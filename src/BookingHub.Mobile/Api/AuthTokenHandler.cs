using System.Net;
using System.Net.Http.Headers;
using BookingHub.Mobile.Services;

namespace BookingHub.Mobile.Api;

/// <summary>
/// Attaches the stored access token to every outgoing request on the "Api" HttpClient, and on a
/// 401, refreshes once (via IAuthApiClient, built on a client without this handler — see its
/// own remarks) and retries the original request exactly once with the new token.
/// </summary>
internal sealed class AuthTokenHandler(ISecureTokenStore tokenStore, IAuthApiClient authApiClient) : DelegatingHandler
{
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await tokenStore.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            // Someone else may have already refreshed while this request waited for the lock —
            // retry with whatever is currently stored before attempting a second refresh.
            var currentAccessToken = await tokenStore.GetAccessTokenAsync();
            if (currentAccessToken != accessToken && !string.IsNullOrEmpty(currentAccessToken))
                return await ResendWithTokenAsync(request, currentAccessToken, cancellationToken);

            var refreshToken = await tokenStore.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
            {
                await tokenStore.ClearAsync();
                return response;
            }

            var refreshed = await authApiClient.RefreshAsync(refreshToken, cancellationToken);
            if (refreshed is null)
            {
                await tokenStore.ClearAsync();
                return response;
            }

            await tokenStore.SaveTokensAsync(refreshed.AccessToken, refreshed.RefreshToken);
            return await ResendWithTokenAsync(request, refreshed.AccessToken, cancellationToken);
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private async Task<HttpResponseMessage> ResendWithTokenAsync(HttpRequestMessage original, string accessToken, CancellationToken cancellationToken)
    {
        // HttpRequestMessage can only be sent once — a retry needs a fresh clone, not the same instance.
        using var retryRequest = await CloneAsync(original);
        retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await base.SendAsync(retryRequest, cancellationToken);
    }

    private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content is not null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);
            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return clone;
    }
}