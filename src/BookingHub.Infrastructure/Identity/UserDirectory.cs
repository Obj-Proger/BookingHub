using BookingHub.Application.Common.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Identity;

internal sealed class UserDirectory(UserManager<ApplicationUser> userManager) : IUserDirectory
{
    public async Task<Guid?> FindUserIdByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user?.Id;
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetEmailsAsync(
        IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken) =>
        await userManager.Users
            .Where(user => userIds.Contains(user.Id) && user.Email != null)
            .ToDictionaryAsync(user => user.Id, user => user.Email!, cancellationToken);
}