using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace BookingHub.Web.Auth;

/// <summary>Ends a circuit's authenticated state as soon as its server-side session is gone.</summary>
internal sealed class SessionAuthenticationStateProvider(ILoggerFactory loggerFactory, ITokenStore tokenStore)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(1);

    protected override Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        var sessionId = authenticationState.User.GetSessionId();
        return Task.FromResult(sessionId is not null && tokenStore.TryGet(sessionId, out _));
    }
}