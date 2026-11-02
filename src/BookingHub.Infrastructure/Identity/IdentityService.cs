using BookingHub.Application.Common.Security;
using Microsoft.AspNetCore.Identity;

namespace BookingHub.Infrastructure.Identity;

internal sealed class IdentityService(UserManager<ApplicationUser> userManager, IPasswordHasher<ApplicationUser> passwordHasher)
    : IIdentityService
{
    private static readonly ApplicationUser DummyUser = new();
    private static readonly string DummyPasswordHash =
        new PasswordHasher<ApplicationUser>().HashPassword(DummyUser, "not-a-real-password");

    public async Task<Result<Guid>> RegisterAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser { UserName = email, Email = email };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            return user.Id;

        // The user name is the email, so a taken address is reported twice (DuplicateUserName, DuplicateEmail).
        var reasons = result.Errors.Select(error => error.Description).Distinct();
        return Result.Failure<Guid>(Error.Validation("Auth.RegistrationRejected", string.Join(" ", reasons)));
    }

    /// <summary>
    /// Always runs a full password verification, even when no account exists for the email —
    /// against a dummy hash — so an unregistered email cannot be told apart from a wrong
    /// password by response time.
    /// </summary>
    public async Task<Guid?> VerifyCredentialsAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        var verification = passwordHasher.VerifyHashedPassword(
            user ?? DummyUser, user?.PasswordHash ?? DummyPasswordHash, password);

        return user is null || verification == PasswordVerificationResult.Failed ? null : user.Id;
    }
}