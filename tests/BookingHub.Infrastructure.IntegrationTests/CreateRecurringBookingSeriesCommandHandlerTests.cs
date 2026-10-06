using BookingHub.Application.Common;
using BookingHub.Application.Features.Bookings.Commands.CreateRecurringBookingSeries;
using BookingHub.Infrastructure.IntegrationTests.TestDoubles;
using BookingHub.Infrastructure.Persistence.Repositories;

namespace BookingHub.Infrastructure.IntegrationTests;

[Collection(nameof(DatabaseCollection))]
public class CreateRecurringBookingSeriesCommandHandlerTests(PostgreSqlFixture fixture)
{
    private static WeeklyHours OpenAllWeek() =>
        WeeklyHours.Create(Enum.GetValues<DayOfWeek>()
            .Select(day => DailyHours.CreateOpen(day, new TimeOnly(0, 0), new TimeOnly(23, 59)).Value)).Value;

    private static List<RecurringSchedule> FullWeekSchedule(Guid assignmentId) =>
        [.. Enum.GetValues<DayOfWeek>().Select(day => RecurringSchedule.Create(assignmentId, day, new TimeOnly(0, 0), new TimeOnly(23, 59)).Value)];

    [Fact]
    public async Task Handle_AllOccurrencesAvailable_CreatesAllBookingsLinkedToOneClient()
    {
        var organization = Organization.Create("Acme", $"acme-{Guid.CreateVersion7()}").Value;
        var location = Location.Create(organization.Id, "Branch", Address.Create("1 First St").Value, "UTC", OpenAllWeek()).Value;
        var employee = Employee.Create(organization.Id, "John Smith").Value;
        var assignment = EmployeeLocationAssignment.Create(employee.Id, location.Id).Value;
        var schedule = FullWeekSchedule(assignment.Id);
        var service = Service.Create(
            organization.Id, "Haircut", TimeSpan.FromMinutes(30), Money.Create(50m, "USD").Value, TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.Locations.Add(location);
            seedContext.Employees.Add(employee);
            seedContext.EmployeeLocationAssignments.Add(assignment);
            seedContext.RecurringSchedules.AddRange(schedule);
            seedContext.Services.Add(service);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new CreateRecurringBookingSeriesCommandHandler(
            dbContext, new ClientRepository(dbContext), new BookingRepository(dbContext),
            new NoOpEmailService(), new NoOpSmsService(), new FakeWebLinkBuilder(), new UnitOfWork(dbContext));

        var firstStart = DateTime.UtcNow.Date.AddDays(14).AddHours(10);
        var result = await handler.Handle(
            new CreateRecurringBookingSeriesCommand(
                organization.Id, organization.Slug, location.Id, employee.Id, service.Id, firstStart,
                IntervalWeeks: 1, OccurrenceCount: 3, Phone: "+14155552680", ClientName: "Jane Doe", ClientEmail: null),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.CreatedBookings.Should().HaveCount(3);
        result.Value.SkippedOccurrenceStartsUtc.Should().BeEmpty();

        await using var verifyContext = fixture.CreateDbContext();
        var clientIds = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
        verifyContext.Bookings
            .Where(b => b.RecurringSeriesId == result.Value.RecurringSeriesId)
            .Select(b => b.ClientId),
            TestContext.Current.CancellationToken);
        clientIds.Should().OnlyContain(id => id != null);
        clientIds.Distinct().Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_OneOccurrenceConflicts_SkipsThatOneAndCreatesTheRest()
    {
        var organization = Organization.Create("Acme", $"acme-{Guid.CreateVersion7()}").Value;
        var location = Location.Create(organization.Id, "Branch", Address.Create("1 First St").Value, "UTC", OpenAllWeek()).Value;
        var employee = Employee.Create(organization.Id, "John Smith").Value;
        var assignment = EmployeeLocationAssignment.Create(employee.Id, location.Id).Value;
        var schedule = FullWeekSchedule(assignment.Id);
        var service = Service.Create(
            organization.Id, "Haircut", TimeSpan.FromMinutes(30), Money.Create(50m, "USD").Value, TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;

        var firstStart = DateTime.UtcNow.Date.AddDays(14).AddHours(10);
        var secondOccurrenceStart = firstStart.AddDays(7);

        var conflictingBooking = Booking.CreatePending(
            organization.Id, location.Id, employee.Id, service.Id,
            ClientContact.Create(PhoneNumber.Create("+14155552681").Value),
            TimeSlot.Create(secondOccurrenceStart, secondOccurrenceStart.AddMinutes(30)).Value,
            Money.Create(50m, "USD").Value, BookingSource.Public, DateTime.UtcNow).Value;
        conflictingBooking.Confirm(DateTime.UtcNow);

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.Locations.Add(location);
            seedContext.Employees.Add(employee);
            seedContext.EmployeeLocationAssignments.Add(assignment);
            seedContext.RecurringSchedules.AddRange(schedule);
            seedContext.Services.Add(service);
            seedContext.Bookings.Add(conflictingBooking);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new CreateRecurringBookingSeriesCommandHandler(
            dbContext, new ClientRepository(dbContext), new BookingRepository(dbContext),
            new NoOpEmailService(), new NoOpSmsService(), new FakeWebLinkBuilder(), new UnitOfWork(dbContext));

        var result = await handler.Handle(
            new CreateRecurringBookingSeriesCommand(
                organization.Id, organization.Slug, location.Id, employee.Id, service.Id, firstStart,
                IntervalWeeks: 1, OccurrenceCount: 3, Phone: "+14155552682", ClientName: "Jane Doe", ClientEmail: null),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.CreatedBookings.Should().HaveCount(2);
        result.Value.SkippedOccurrenceStartsUtc.Should().ContainSingle().Which.Should().Be(secondOccurrenceStart);
    }

    [Fact]
    public async Task Handle_EmployeeNotAssignedToLocation_ReturnsNoOccurrenceAvailable()
    {
        var organization = Organization.Create("Acme", $"acme-{Guid.CreateVersion7()}").Value;
        var location = Location.Create(organization.Id, "Branch", Address.Create("1 First St").Value, "UTC", OpenAllWeek()).Value;
        var employee = Employee.Create(organization.Id, "John Smith").Value;
        var service = Service.Create(
            organization.Id, "Haircut", TimeSpan.FromMinutes(30), Money.Create(50m, "USD").Value, TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.Locations.Add(location);
            seedContext.Employees.Add(employee);
            seedContext.Services.Add(service);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new CreateRecurringBookingSeriesCommandHandler(
            dbContext, new ClientRepository(dbContext), new BookingRepository(dbContext),
            new NoOpEmailService(), new NoOpSmsService(), new FakeWebLinkBuilder(), new UnitOfWork(dbContext));

        var result = await handler.Handle(
            new CreateRecurringBookingSeriesCommand(
                organization.Id, organization.Slug, location.Id, employee.Id, service.Id, DateTime.UtcNow.Date.AddDays(14).AddHours(10),
                IntervalWeeks: 1, OccurrenceCount: 3, Phone: "+14155552683", ClientName: "Jane Doe", ClientEmail: null),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Booking.NoOccurrenceAvailable);
    }
}