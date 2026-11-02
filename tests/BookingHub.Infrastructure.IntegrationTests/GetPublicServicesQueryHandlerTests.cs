using BookingHub.Application.Features.Public.Queries.GetPublicServices;

namespace BookingHub.Infrastructure.IntegrationTests;

[Collection(nameof(DatabaseCollection))]
public class GetPublicServicesQueryHandlerTests(PostgreSqlFixture fixture)
{
    private static WeeklyHours OpenAllWeek() =>
        WeeklyHours.Create(Enum.GetValues<DayOfWeek>()
            .Select(day => DailyHours.CreateOpen(day, new TimeOnly(0, 0), new TimeOnly(23, 59)).Value)).Value;

    private static Service CreateService(Guid organizationId, string name, decimal price) =>
        Service.Create(
            organizationId, name, TimeSpan.FromMinutes(30), Money.Create(price, "USD").Value, TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;

    [Fact]
    public async Task Handle_OverrideAtTheLocation_ReturnsServicesByNameWithTheLocationPrice()
    {
        var organization = Organization.Create("Acme", $"acme-{Guid.CreateVersion7()}").Value;
        var location = Location.Create(organization.Id, "Central", Address.Create("1 First St").Value, "UTC", OpenAllWeek()).Value;
        var otherLocation = Location.Create(organization.Id, "Riverside", Address.Create("2 Second St").Value, "UTC", OpenAllWeek()).Value;
        var haircut = CreateService(organization.Id, "Haircut", 50m);
        var beardTrim = CreateService(organization.Id, "Beard trim", 20m);
        var haircutAtLocation = LocationServiceOverride.Create(location.Id, haircut.Id, Money.Create(65m, "USD").Value).Value;
        var beardTrimElsewhere = LocationServiceOverride.Create(otherLocation.Id, beardTrim.Id, Money.Create(99m, "USD").Value).Value;

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.Locations.AddRange(location, otherLocation);
            seedContext.Services.AddRange(haircut, beardTrim);
            seedContext.LocationServiceOverrides.AddRange(haircutAtLocation, beardTrimElsewhere);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new GetPublicServicesQueryHandler(dbContext);

        var result = await handler.Handle(
            new GetPublicServicesQuery(organization.Id, location.Id), TestContext.Current.CancellationToken);

        result.Value.Select(s => (s.Name, s.PriceAmount)).Should().Equal(("Beard trim", 20m), ("Haircut", 65m));
    }
}
