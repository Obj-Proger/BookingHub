using BookingHub.Application.Common;
using BookingHub.Application.Features.Bookings.Commands.RescheduleBooking;
using BookingHub.Infrastructure.Persistence.Repositories;

namespace BookingHub.Infrastructure.IntegrationTests;

[Collection(nameof(DatabaseCollection))]
public class RescheduleBookingCommandHandlerTests(PostgreSqlFixture fixture)
{
    private static WeeklyHours OpenAllWeek() =>
        WeeklyHours.Create(Enum.GetValues<DayOfWeek>()
            .Select(day => DailyHours.CreateOpen(day, new TimeOnly(0, 0), new TimeOnly(23, 59)).Value)).Value;

    private static List<RecurringSchedule> FullWeekSchedule(Guid assignmentId) =>
        [.. Enum.GetValues<DayOfWeek>().Select(day => RecurringSchedule.Create(assignmentId, day, new TimeOnly(0, 0), new TimeOnly(23, 59)).Value)];

    private sealed record Fixture_(Organization Organization, Location Location, Employee Employee, Service Service, Booking Booking);

    private async Task<Fixture_> SeedConfirmedBookingAsync(DateTime slotStartUtc, string phone)
    {
        var organization = Organization.Create("Acme", $"acme-{Guid.CreateVersion7()}").Value;
        var location = Location.Create(organization.Id, "Branch", Address.Create("1 First St").Value, "UTC", OpenAllWeek()).Value;
        var employee = Employee.Create(organization.Id, "John Smith").Value;
        var assignment = EmployeeLocationAssignment.Create(employee.Id, location.Id).Value;
        var schedule = FullWeekSchedule(assignment.Id);
        var service = Service.Create(
            organization.Id, "Haircut", TimeSpan.FromMinutes(30), Money.Create(50m, "USD").Value, TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;
        var booking = Booking.CreatePending(
            organization.Id, location.Id, employee.Id, service.Id, ClientContact.Create(PhoneNumber.Create(phone).Value),
            TimeSlot.Create(slotStartUtc, slotStartUtc.AddMinutes(30)).Value,
            Money.Create(50m, "USD").Value, BookingSource.Public, DateTime.UtcNow).Value;
        booking.Confirm(DateTime.UtcNow);

        await using var seedContext = fixture.CreateDbContext();
        seedContext.Organizations.Add(organization);
        seedContext.Locations.Add(location);
        seedContext.Employees.Add(employee);
        seedContext.EmployeeLocationAssignments.Add(assignment);
        seedContext.RecurringSchedules.AddRange(schedule);
        seedContext.Services.Add(service);
        seedContext.Bookings.Add(booking);
        await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return new Fixture_(organization, location, employee, service, booking);
    }

    [Fact]
    public async Task Handle_ValidTokenAndAvailableSlot_ReschedulesBooking()
    {
        var originalStart = DateTime.UtcNow.Date.AddDays(14).AddHours(10);
        var f = await SeedConfirmedBookingAsync(originalStart, "+14155552684");
        var newStart = originalStart.AddHours(2);

        await using var dbContext = fixture.CreateDbContext();
        var handler = new RescheduleBookingCommandHandler(new BookingRepository(dbContext), dbContext, new UnitOfWork(dbContext));

        var result = await handler.Handle(
            new RescheduleBookingCommand(f.Booking.Id, f.Booking.CancellationToken.Value, newStart), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.StartUtc.Should().Be(newStart);
    }

    [Fact]
    public async Task Handle_InvalidToken_ReturnsInvalidManagementToken()
    {
        var originalStart = DateTime.UtcNow.Date.AddDays(14).AddHours(10);
        var f = await SeedConfirmedBookingAsync(originalStart, "+14155552685");

        await using var dbContext = fixture.CreateDbContext();
        var handler = new RescheduleBookingCommandHandler(new BookingRepository(dbContext), dbContext, new UnitOfWork(dbContext));

        var result = await handler.Handle(
            new RescheduleBookingCommand(f.Booking.Id, "wrong-token", originalStart.AddHours(2)), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Booking.InvalidManagementToken);
    }

    [Fact]
    public async Task Handle_WithinCancellationDeadline_ReturnsCancellationDeadlinePassed()
    {
        var originalStart = DateTime.UtcNow.AddHours(2); // well inside the default 24h deadline
        var f = await SeedConfirmedBookingAsync(originalStart, "+14155552686");

        await using var dbContext = fixture.CreateDbContext();
        var handler = new RescheduleBookingCommandHandler(new BookingRepository(dbContext), dbContext, new UnitOfWork(dbContext));

        var result = await handler.Handle(
            new RescheduleBookingCommand(f.Booking.Id, f.Booking.CancellationToken.Value, originalStart.AddHours(4)),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Booking.CancellationDeadlinePassed);
    }

    [Fact]
    public async Task Handle_NewSlotOccupiedByAnotherBooking_ReturnsSlotNotAvailable()
    {
        var originalStart = DateTime.UtcNow.Date.AddDays(14).AddHours(10);
        var f = await SeedConfirmedBookingAsync(originalStart, "+14155552687");
        var newStart = originalStart.AddHours(4);

        var otherBooking = Booking.CreatePending(
            f.Organization.Id, f.Location.Id, f.Employee.Id, f.Service.Id, ClientContact.Create(PhoneNumber.Create("+14155552688").Value),
            TimeSlot.Create(newStart, newStart.AddMinutes(30)).Value,
            Money.Create(50m, "USD").Value, BookingSource.Public, DateTime.UtcNow).Value;
        otherBooking.Confirm(DateTime.UtcNow);

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Bookings.Add(otherBooking);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new RescheduleBookingCommandHandler(new BookingRepository(dbContext), dbContext, new UnitOfWork(dbContext));

        var result = await handler.Handle(
            new RescheduleBookingCommand(f.Booking.Id, f.Booking.CancellationToken.Value, newStart), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Booking.SlotNotAvailable);
    }
}