namespace BookingHub.Application.Common.Security;

/// <summary>Read access to the registered users, which live in the Identity store outside the Application layer.</summary>
public interface IUserDirectory
{
    /// <returns>The id of the user registered with this email address, or null if there is none.</returns>
    Task<Guid?> FindUserIdByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>Email addresses by user id; users that no longer exist are simply absent.</summary>
    Task<IReadOnlyDictionary<Guid, string>> GetEmailsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken);
}