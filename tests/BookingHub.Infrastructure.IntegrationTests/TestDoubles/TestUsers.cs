using BookingHub.Infrastructure.Identity;

namespace BookingHub.Infrastructure.IntegrationTests.TestDoubles;

/// <summary>Identity users for seeding: memberships must reference a registered user.</summary>
internal static class TestUsers
{
    public static ApplicationUser Create()
    {
        var id = Guid.CreateVersion7();
        var email = $"user-{id}@example.com";
        return new ApplicationUser { Id = id, UserName = email, Email = email };
    }
}
