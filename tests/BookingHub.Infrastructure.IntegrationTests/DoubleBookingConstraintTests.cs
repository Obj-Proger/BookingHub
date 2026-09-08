using BookingHub.Application.Common.Persistence;

namespace BookingHub.Infrastructure.IntegrationTests;

[Collection(nameof(DatabaseCollection))]
public class DoubleBookingConstraintTests(PostgreSqlFixture fixture)
{
    [Fact]
    public async Task SaveChangesAsync_TwoConfirmedOverlappingBookingsForSameEmployee_ThrowsConcurrencyConflictException()
    {
        var organization = Organization.Create("Org", $"org-{Guid.CreateVersion7()}").Value;
        var location = Location.Create(
            organization.Id, "Branch", Address.Create("221B Baker Street").Value, "UTC",
            WeeklyHours.Create(Enum.GetValues<DayOfWeek>().Select(DailyHours.CreateClosed)).Value).Value;
        var employee = Employee.Create(organization.Id, "Jane Doe").Value;
        var service = Service.Create(
            organization.Id, "Haircut", TimeSpan.FromMinutes(30), Money.Create(50m, "USD").Value,
            TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;
        var price = Money.Create(50m, "USD").Value;
        var utcNow = DateTime.UtcNow;

        var firstBooking = Booking.CreatePending(
            organization.Id, location.Id, employee.Id, service.Id,
            ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
            TimeSlot.Create(utcNow.AddHours(1), utcNow.AddHours(2)).Value, price, BookingSource.Public, utcNow).Value;
        firstBooking.Confirm(utcNow);

        var secondBooking = Booking.CreatePending(
            organization.Id, location.Id, employee.Id, service.Id,
            ClientContact.Create(PhoneNumber.Create("+14155559999").Value),
            TimeSlot.Create(utcNow.AddMinutes(90), utcNow.AddMinutes(150)).Value, price, BookingSource.Public, utcNow).Value;
        secondBooking.Confirm(utcNow);

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.Locations.Add(location);
            seedContext.Employees.Add(employee);
            seedContext.Services.Add(service);
            seedContext.Bookings.Add(firstBooking);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        dbContext.Bookings.Add(secondBooking);
        var unitOfWork = new UnitOfWork(dbContext);

        var act = async () => await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ConcurrencyConflictException>();
    }

    [Fact]
    public async Task SaveChangesAsync_TwoConfirmedNonOverlappingBookingsForSameEmployee_Succeeds()
    {
        var organization = Organization.Create("Org", $"org-{Guid.CreateVersion7()}").Value;
        var location = Location.Create(
            organization.Id, "Branch", Address.Create("221B Baker Street").Value, "UTC",
            WeeklyHours.Create(Enum.GetValues<DayOfWeek>().Select(DailyHours.CreateClosed)).Value).Value;
        var employee = Employee.Create(organization.Id, "Jane Doe").Value;
        var service = Service.Create(
            organization.Id, "Haircut", TimeSpan.FromMinutes(30), Money.Create(50m, "USD").Value,
            TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;
        var price = Money.Create(50m, "USD").Value;
        var utcNow = DateTime.UtcNow;

        var firstBooking = Booking.CreatePending(
            organization.Id, location.Id, employee.Id, service.Id,
            ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
            TimeSlot.Create(utcNow.AddHours(1), utcNow.AddHours(2)).Value, price, BookingSource.Public, utcNow).Value;
        firstBooking.Confirm(utcNow);

        var secondBooking = Booking.CreatePending(
            organization.Id, location.Id, employee.Id, service.Id,
            ClientContact.Create(PhoneNumber.Create("+14155559999").Value),
            TimeSlot.Create(utcNow.AddHours(2), utcNow.AddHours(3)).Value, price, BookingSource.Public, utcNow).Value;
        secondBooking.Confirm(utcNow);

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.Locations.Add(location);
            seedContext.Employees.Add(employee);
            seedContext.Services.Add(service);
            seedContext.Bookings.Add(firstBooking);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        dbContext.Bookings.Add(secondBooking);
        var unitOfWork = new UnitOfWork(dbContext);

        var act = async () => await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
    }
}