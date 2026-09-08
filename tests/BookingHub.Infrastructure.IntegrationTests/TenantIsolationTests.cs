using BookingHub.Infrastructure.Persistence.Repositories;

namespace BookingHub.Infrastructure.IntegrationTests;

[Collection(nameof(DatabaseCollection))]
public class TenantIsolationTests(PostgreSqlFixture fixture)
{
    private static WeeklyHours ClosedAllWeek() =>
        WeeklyHours.Create(Enum.GetValues<DayOfWeek>().Select(DailyHours.CreateClosed)).Value;

    [Fact]
    public async Task LocationRepository_GetById_CannotReadAnotherOrganizationsLocation()
    {
        var organizationA = Organization.Create("Org A", $"org-a-{Guid.CreateVersion7()}").Value;
        var organizationB = Organization.Create("Org B", $"org-b-{Guid.CreateVersion7()}").Value;
        var locationB = Location.Create(
            organizationB.Id, "Org B Branch", Address.Create("221B Baker Street").Value, "UTC", ClosedAllWeek()).Value;

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.AddRange(organizationA, organizationB);
            seedContext.Locations.Add(locationB);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var repository = new LocationRepository(dbContext);

        var result = await repository.GetByIdAsync(organizationA.Id, locationB.Id, TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ReviewRepository_GetById_CannotReadAnotherLocationsReview()
    {
        var organization = Organization.Create("Org", $"org-{Guid.CreateVersion7()}").Value;
        var locationA = Location.Create(organization.Id, "Branch A", Address.Create("1 First St").Value, "UTC", ClosedAllWeek()).Value;
        var locationB = Location.Create(organization.Id, "Branch B", Address.Create("2 Second St").Value, "UTC", ClosedAllWeek()).Value;
        var employee = Employee.Create(organization.Id, "Jane Doe").Value;
        var service = Service.Create(
            organization.Id, "Haircut", TimeSpan.FromMinutes(30), Money.Create(50m, "USD").Value,
            TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;
        var booking = Booking.CreatePending(
            organization.Id, locationB.Id, employee.Id, service.Id,
            ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
            TimeSlot.Create(DateTime.UtcNow.AddHours(-3), DateTime.UtcNow.AddHours(-2)).Value,
            Money.Create(50m, "USD").Value, BookingSource.Public, DateTime.UtcNow.AddHours(-4)).Value;
        var review = Review.Create(organization.Id, locationB.Id, employee.Id, booking.Id, 5, null, DateTime.UtcNow).Value;

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.Locations.AddRange(locationA, locationB);
            seedContext.Employees.Add(employee);
            seedContext.Services.Add(service);
            seedContext.Bookings.Add(booking);
            seedContext.Reviews.Add(review);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var repository = new ReviewRepository(dbContext);

        var result = await repository.GetByIdAsync(locationA.Id, review.Id, TestContext.Current.CancellationToken);

        result.Should().BeNull();
    }
}