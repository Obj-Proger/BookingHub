using BookingHub.API.Common;
using BookingHub.API.RateLimiting;
using BookingHub.Application.Features.Auth.Commands.Login;
using BookingHub.Application.Features.Auth.Commands.Logout;
using BookingHub.Application.Features.Auth.Commands.RefreshTokens;
using BookingHub.Application.Features.Auth.Commands.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BookingHub.API.Controllers.Auth;

[AllowAnonymous]
[Route("api/v1/auth")]
public sealed class AuthController(IDispatcher dispatcher) : ApiControllerBase
{
    [EnableRateLimiting(RateLimitPolicies.SignIn)]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new RegisterCommand(request.Email, request.Password), cancellationToken);
        return HandleResult(result);
    }

    [EnableRateLimiting(RateLimitPolicies.SignIn)]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        return HandleResult(result);
    }

    [EnableRateLimiting(RateLimitPolicies.Session)]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new RefreshTokensCommand(request.RefreshToken), cancellationToken);
        return HandleResult(result);
    }

    [EnableRateLimiting(RateLimitPolicies.Session)]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
        return HandleResult(result);
    }
}