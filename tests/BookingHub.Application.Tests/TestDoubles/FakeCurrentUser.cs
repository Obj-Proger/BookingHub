using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Tests.TestDoubles;

internal sealed class FakeCurrentUser(Guid userId) : ICurrentUser
{
    public Guid UserId { get; } = userId;
}