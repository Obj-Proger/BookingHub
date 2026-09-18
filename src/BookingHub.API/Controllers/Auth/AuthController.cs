using BookingHub.API.Common;
using BookingHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BookingHub.API.Controllers.Auth;

[AllowAnonymous]
[Route("api/v1/auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    IPasswordHasher<ApplicationUser> passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenService refreshTokenService)
    : ApiControllerBase
{
    private static readonly ApplicationUser DummyUser = new();
    private static readonly string DummyPasswordHash =
        new PasswordHasher<ApplicationUser>().HashPassword(DummyUser, "not-a-real-password");

    [EnableRateLimiting("public-write")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
                ModelState.AddModelError(error.Code, error.Description);

            return ValidationProblem(ModelState);
        }

        return await IssueTokensAsync(user);
    }

    /// <summary>
    /// Always runs a full password verification, even when no account exists for the given
    /// email — checking a dummy hash in that case, instead of returning immediately — so a
    /// nonexistent email can't be distinguished from a wrong password by response time alone.
    /// </summary>
    [EnableRateLimiting("public-write")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        var passwordVerification = passwordHasher.VerifyHashedPassword(
            user ?? DummyUser, user?.PasswordHash ?? DummyPasswordHash, request.Password);

        if (user is null || passwordVerification == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Invalid email or password.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        return await IssueTokensAsync(user);
    }

    [EnableRateLimiting("public-write")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var validation = await refreshTokenService.ValidateAndRotateAsync(request.RefreshToken, cancellationToken);
        if (!validation.Succeeded)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Invalid or expired refresh token.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var user = await userManager.FindByIdAsync(validation.UserId.ToString());
        if (user is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Invalid or expired refresh token.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        return Ok(new AuthenticatedResponse(jwtTokenGenerator.GenerateToken(user), validation.NewRawToken!));
    }

    [EnableRateLimiting("public-write")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    private async Task<IActionResult> IssueTokensAsync(ApplicationUser user)
    {
        var accessToken = jwtTokenGenerator.GenerateToken(user);
        var refreshToken = await refreshTokenService.IssueAsync(user.Id, CancellationToken.None);
        return Ok(new AuthenticatedResponse(accessToken, refreshToken));
    }
}