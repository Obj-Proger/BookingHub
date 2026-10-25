using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookingHub.Web.Api;
using BookingHub.Web.Api.Contracts;
using Microsoft.AspNetCore.Components.Authorization;

namespace BookingHub.Web.Auth;

/// <summary>
/// Sends API requests on behalf of the signed-in user. Must stay scoped (one per circuit):
/// it reads the user's session from the scoped <see cref="AuthenticationStateProvider"/>,
/// which a typed client or a message handler could not reach.
/// A missing or unrefreshable session is reported as a 401 response, an unreachable API as a 503.
/// </summary>
internal sealed class AuthorizedApiClient(
    IHttpClientFactory httpClientFactory,
    AuthenticationStateProvider authenticationStateProvider,
    ITokenStore tokenStore,
    IAuthApiClient authApiClient)
{
    public const string HttpClientName = "api-authorized";

    public Task<ApiResult<T>> GetJsonAsync<T>(string uri, CancellationToken cancellationToken) =>
        SendJsonAsync<T>(HttpMethod.Get, uri, body: null, cancellationToken);

    /// <summary>Sends <paramref name="body"/> as JSON (if any) and reads the JSON response as <typeparamref name="T"/>.</summary>
    public async Task<ApiResult<T>> SendJsonAsync<T>(HttpMethod method, string uri, object? body, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(() => CreateJsonRequest(method, uri, body), cancellationToken);
        return await ApiResponseReader.ReadAsync<T>(response, cancellationToken);
    }

    /// <summary>Sends <paramref name="body"/> as JSON (if any) to an endpoint that answers without a body.</summary>
    public async Task<ApiResult> SendJsonAsync(HttpMethod method, string uri, object? body, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(() => CreateJsonRequest(method, uri, body), cancellationToken);
        return await ApiResponseReader.ReadAsync(response, cancellationToken);
    }

    /// <param name="createRequest">
    /// A factory rather than a request: an <see cref="HttpRequestMessage"/> cannot be sent twice,
    /// and a 401 is retried once after refreshing the tokens.
    /// </param>
    public async Task<HttpResponseMessage> SendAsync(Func<HttpRequestMessage> createRequest, CancellationToken cancellationToken)
    {
        try
        {
            return await SendWithRefreshAsync(createRequest, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        }
    }

    private static HttpRequestMessage CreateJsonRequest(HttpMethod method, string uri, object? body) =>
        new(method, uri)
        {
            Content = body is null ? null : JsonContent.Create(body, body.GetType(), options: ApiJsonOptions.Default)
        };

    private async Task<HttpResponseMessage> SendWithRefreshAsync(Func<HttpRequestMessage> createRequest, CancellationToken cancellationToken)
    {
        var authenticationState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var sessionId = authenticationState.User.GetSessionId();

        if (sessionId is null || !tokenStore.TryGet(sessionId, out var session))
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);

        var tokens = session.Tokens;
        var response = await SendWithTokenAsync(createRequest, tokens.AccessToken, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        response.Dispose();

        var refreshed = await RefreshAsync(sessionId, session, tokens.RefreshToken, cancellationToken);
        return refreshed is null
            ? new HttpResponseMessage(HttpStatusCode.Unauthorized)
            : await SendWithTokenAsync(createRequest, refreshed.AccessToken, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendWithTokenAsync(
        Func<HttpRequestMessage> createRequest, string accessToken, CancellationToken cancellationToken)
    {
        using var request = createRequest();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await httpClientFactory.CreateClient(HttpClientName).SendAsync(request, cancellationToken);
    }

    private async Task<AuthenticatedResponse?> RefreshAsync(
        string sessionId, TokenSession session, string staleRefreshToken, CancellationToken cancellationToken)
    {
        await session.RefreshLock.WaitAsync(cancellationToken);
        try
        {
            // Refresh tokens are single-use: replaying an already rotated one makes the API revoke all
            // of the user's tokens, so a refresh that another request completed meanwhile is reused.
            if (session.Tokens.RefreshToken != staleRefreshToken)
                return session.Tokens;

            var refreshed = await authApiClient.RefreshAsync(staleRefreshToken, cancellationToken);
            if (refreshed is null)
            {
                tokenStore.Remove(sessionId);
                return null;
            }

            tokenStore.Update(sessionId, refreshed);
            return refreshed;
        }
        finally
        {
            session.RefreshLock.Release();
        }
    }
}