using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.IntegrationTests;

[Collection(nameof(DatabaseCollection))]
public class RlsPolicyTests(PostgreSqlFixture fixture)
{
    private static WeeklyHours ClosedAllWeek() =>
        WeeklyHours.Create(Enum.GetValues<DayOfWeek>().Select(DailyHours.CreateClosed)).Value;

    [Fact]
    public async Task AppRole_NoOrganizationContextSet_SeesNoRowsAtAll()
    {
        var organization = Organization.Create("Org", $"org-{Guid.CreateVersion7()}").Value;
        var location = Location.Create(organization.Id, "Branch", Address.Create("221B Baker Street").Value, "UTC", ClosedAllWeek()).Value;

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.Locations.Add(location);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var appRoleContext = fixture.CreateAppRoleDbContext();

        var locations = await appRoleContext.Locations.ToListAsync(TestContext.Current.CancellationToken);

        locations.Should().BeEmpty();
    }

    [Fact]
    public async Task AppRole_MatchingOrganizationContextSet_SeesOwnRows()
    {
        var organization = Organization.Create("Org", $"org-{Guid.CreateVersion7()}").Value;
        var location = Location.Create(organization.Id, "Branch", Address.Create("221B Baker Street").Value, "UTC", ClosedAllWeek()).Value;

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.Locations.Add(location);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var appRoleContext = fixture.CreateAppRoleDbContext();
        await using var transaction = await appRoleContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

        // is_local = true, matching TenantResolutionMiddleware's real behavior exactly — the
        // setting is scoped to this transaction alone and cannot leak to the next test via the
        // pooled connection, unlike the session-scoped (false) version this replaced.
        await appRoleContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.current_organization_id', {organization.Id.ToString()}, true)",
            TestContext.Current.CancellationToken);

        var locations = await appRoleContext.Locations.ToListAsync(TestContext.Current.CancellationToken);

        locations.Should().ContainSingle(l => l.Id == location.Id);
    }

    [Fact]
    public async Task AppRole_DifferentOrganizationContextSet_SeesNoRows()
    {
        var organizationA = Organization.Create("Org A", $"org-a-{Guid.CreateVersion7()}").Value;
        var organizationB = Organization.Create("Org B", $"org-b-{Guid.CreateVersion7()}").Value;
        var locationB = Location.Create(organizationB.Id, "Branch", Address.Create("221B Baker Street").Value, "UTC", ClosedAllWeek()).Value;

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.AddRange(organizationA, organizationB);
            seedContext.Locations.Add(locationB);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var appRoleContext = fixture.CreateAppRoleDbContext();
        await using var transaction = await appRoleContext.Database.BeginTransactionAsync(TestContext.Current.CancellationToken);

        await appRoleContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.current_organization_id', {organizationA.Id.ToString()}, true)",
            TestContext.Current.CancellationToken);

        var locations = await appRoleContext.Locations.ToListAsync(TestContext.Current.CancellationToken);

        locations.Should().BeEmpty();
    }
}