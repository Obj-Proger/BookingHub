using System.Security.Claims;
using BookingHub.Web.Api;
using BookingHub.Web.Api.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BookingHub.Web.Auth;

/// <summary>Creates and tears down a staff session: the server-side token entry plus the browser cookie.</summary>
internal sealed class StaffSessionManager(ITokenStore tokenStore, IAuthApiClient authApiClient)
{
    /// <returns>False when the access token's subject cannot be read; nothing is signed in then.</returns>
    public async Task<bool> SignInAsync(
        HttpContext httpContext, string email, AuthenticatedResponse tokens, CancellationToken cancellationToken)
    {
        if (!JwtSubjectReader.TryRead(tokens.AccessToken, out var userId))
        {
            await authApiClient.LogoutAsync(tokens.RefreshToken, cancellationToken);
            return false;
        }

        var sessionId = tokenStore.Create(tokens);

        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email),
                new Claim(StaffClaimTypes.SessionId, sessionId)
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return true;
    }

    public async Task SignOutAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var sessionId = httpContext.User.GetSessionId();

        if (sessionId is not null && tokenStore.TryGet(sessionId, out var session))
        {
            await authApiClient.LogoutAsync(session.Tokens.RefreshToken, cancellationToken);
            tokenStore.Remove(sessionId);
        }

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}