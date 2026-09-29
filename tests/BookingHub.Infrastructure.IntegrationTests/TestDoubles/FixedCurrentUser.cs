using BookingHub.Application.Common.Security;

namespace BookingHub.Infrastructure.IntegrationTests.TestDoubles;

internal sealed class FixedCurrentUser(Guid userId) : ICurrentUser
{
    public Guid UserId => userId;
}