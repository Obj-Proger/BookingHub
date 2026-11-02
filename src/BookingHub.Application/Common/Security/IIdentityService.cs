namespace BookingHub.Application.Common.Security;

/// <summary>User accounts — email and password — which live in the Identity store outside the Application layer.</summary>
public interface IIdentityService
{
    /// <returns>The new user's id, or a validation error listing why the email or password was rejected.</returns>
    Task<Result<Guid>> RegisterAsync(string email, string password, CancellationToken cancellationToken);

    /// <returns>The user's id when the password matches, otherwise null — whether or not the email is registered.</returns>
    Task<Guid?> VerifyCredentialsAsync(string email, string password, CancellationToken cancellationToken);
}