using System.Net;
using System.Net.Http.Headers;
using BookingHub.Web.Api;
using BookingHub.Web.Api.Contracts;
using Microsoft.AspNetCore.Components.Authorization;

namespace BookingHub.Web.Auth;

/// <summary>
/// Sends API requests on behalf of the signed-in user. Must stay scoped (one per circuit):
/// it reads the user's session from the scoped <see cref="AuthenticationStateProvider"/>,
/// which a typed client or a message handler could not reach.
/// A missing or unrefreshable session is reported as a 401 response.
/// </summary>
internal sealed class AuthorizedApiClient(
    IHttpClientFactory httpClientFactory,
    AuthenticationStateProvider authenticationStateProvider,
    ITokenStore tokenStore,
    IAuthApiClient authApiClient)
{
    public const string HttpClientName = "api-authorized";

    /// <param name="createRequest">
    /// A factory rather than a request: an <see cref="HttpRequestMessage"/> cannot be sent twice,
    /// and a 401 is retried once after refreshing the tokens.
    /// </param>
    public async Task<HttpResponseMessage> SendAsync(Func<HttpRequestMessage> createRequest, CancellationToken cancellationToken)
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