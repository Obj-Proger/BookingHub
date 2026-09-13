using BookingHub.API.Common;
using BookingHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace BookingHub.API.Controllers.Auth;

[Route("api/v1/auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    IPasswordHasher<ApplicationUser> passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator)
    : ApiControllerBase
{
    // Generated once via the real hasher (not hand-typed) so its format is guaranteed valid for
    // VerifyHashedPassword — used only to keep Login's timing indistinguishable between "no such
    // user" and "wrong password" (see class-level remark on Login).
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

        return Ok(new AuthenticatedResponse(jwtTokenGenerator.GenerateToken(user)));
    }

    /// <summary>
    /// Always runs a full password verification, even when no account exists for the given
    /// email — checking a dummy hash in that case, instead of returning immediately — so a
    /// nonexistent email can't be distinguished from a wrong password by response time alone
    /// (password hashing is deliberately slow; skipping it for "no such user" would make that
    /// path measurably faster).
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

        return Ok(new AuthenticatedResponse(jwtTokenGenerator.GenerateToken(user)));
    }
}