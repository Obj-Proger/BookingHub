using BookingHub.Application.Common;
using BookingHub.Application.Features.Waitlist.Commands.ConfirmWaitlistOffer;
using BookingHub.Infrastructure.Persistence.Repositories;

namespace BookingHub.Infrastructure.IntegrationTests;

[Collection(nameof(DatabaseCollection))]
public class ConfirmWaitlistOfferCommandHandlerTests(PostgreSqlFixture fixture)
{
    private static WeeklyHours OpenAllWeek() =>
        WeeklyHours.Create(Enum.GetValues<DayOfWeek>()
            .Select(day => DailyHours.CreateOpen(day, new TimeOnly(0, 0), new TimeOnly(23, 59)).Value)).Value;

    private static List<RecurringSchedule> FullWeekSchedule(Guid assignmentId) =>
        [.. Enum.GetValues<DayOfWeek>().Select(day => RecurringSchedule.Create(assignmentId, day, new TimeOnly(0, 0), new TimeOnly(23, 59)).Value)];

    private sealed record Fixture_(Organization Organization, Location Location, Employee Employee, Service Service);

    private async Task<Fixture_> SeedOrgAsync()
    {
        var organization = Organization.Create("Acme", $"acme-{Guid.CreateVersion7()}").Value;
        var location = Location.Create(organization.Id, "Branch", Address.Create("1 First St").Value, "UTC", OpenAllWeek()).Value;
        var employee = Employee.Create(organization.Id, "John Smith").Value;
        var assignment = EmployeeLocationAssignment.Create(employee.Id, location.Id).Value;
        var schedule = FullWeekSchedule(assignment.Id);
        var service = Service.Create(
            organization.Id, "Haircut", TimeSpan.FromMinutes(30), Money.Create(50m, "USD").Value, TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;

        await using var seedContext = fixture.CreateDbContext();
        seedContext.Organizations.Add(organization);
        seedContext.Locations.Add(location);
        seedContext.Employees.Add(employee);
        seedContext.EmployeeLocationAssignments.Add(assignment);
        seedContext.RecurringSchedules.AddRange(schedule);
        seedContext.Services.Add(service);
        await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        return new Fixture_(organization, location, employee, service);
    }

    private static WaitlistEntry CreateOfferedEntry(Fixture_ f, TimeSlot offeredSlot, string phone)
    {
        var entry = WaitlistEntry.Create(
            f.Organization.Id, f.Location.Id, f.Employee.Id, f.Service.Id,
            ClientContact.Create(PhoneNumber.Create(phone).Value), offeredSlot, DateTime.UtcNow).Value;
        entry.Offer(f.Employee.Id, offeredSlot, DateTime.UtcNow.AddHours(1), DateTime.UtcNow);
        return entry;
    }

    [Fact]
    public async Task Handle_ValidTokenAndSlotStillFree_ConvertsEntryAndCreatesConfirmedBooking()
    {
        var f = await SeedOrgAsync();
        var slot = TimeSlot.Create(DateTime.UtcNow.Date.AddDays(14).AddHours(10), DateTime.UtcNow.Date.AddDays(14).AddHours(10).AddMinutes(30)).Value;
        var entry = CreateOfferedEntry(f, slot, "+14155552689");

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.WaitlistEntries.Add(entry);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new ConfirmWaitlistOfferCommandHandler(
            new WaitlistEntryRepository(dbContext), dbContext, new ClientRepository(dbContext), new BookingRepository(dbContext), new UnitOfWork(dbContext));

        var result = await handler.Handle(
            new ConfirmWaitlistOfferCommand(entry.Id, entry.ManagementToken.Value), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(BookingStatus.Confirmed);

        await using var verifyContext = fixture.CreateDbContext();
        var updatedEntry = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstAsync(
            verifyContext.WaitlistEntries, e => e.Id == entry.Id, TestContext.Current.CancellationToken);
        updatedEntry.Status.Should().Be(WaitlistEntryStatus.Converted);
    }

    [Fact]
    public async Task Handle_InvalidToken_ReturnsInvalidManagementToken()
    {
        var f = await SeedOrgAsync();
        var slot = TimeSlot.Create(DateTime.UtcNow.Date.AddDays(14).AddHours(10), DateTime.UtcNow.Date.AddDays(14).AddHours(10).AddMinutes(30)).Value;
        var entry = CreateOfferedEntry(f, slot, "+14155552690");

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.WaitlistEntries.Add(entry);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new ConfirmWaitlistOfferCommandHandler(
            new WaitlistEntryRepository(dbContext), dbContext, new ClientRepository(dbContext), new BookingRepository(dbContext), new UnitOfWork(dbContext));

        var result = await handler.Handle(new ConfirmWaitlistOfferCommand(entry.Id, "wrong-token"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.WaitlistEntry.InvalidManagementToken);
    }

    [Fact]
    public async Task Handle_EntryNotInOfferedStatus_ReturnsCannotConvert()
    {
        var f = await SeedOrgAsync();
        var slot = TimeSlot.Create(DateTime.UtcNow.Date.AddDays(14).AddHours(10), DateTime.UtcNow.Date.AddDays(14).AddHours(10).AddMinutes(30)).Value;
        var entry = WaitlistEntry.Create(
            f.Organization.Id, f.Location.Id, f.Employee.Id, f.Service.Id,
            ClientContact.Create(PhoneNumber.Create("+14155552691").Value), slot, DateTime.UtcNow).Value; // still Waiting, never offered

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.WaitlistEntries.Add(entry);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new ConfirmWaitlistOfferCommandHandler(
            new WaitlistEntryRepository(dbContext), dbContext, new ClientRepository(dbContext), new BookingRepository(dbContext), new UnitOfWork(dbContext));

        var result = await handler.Handle(
            new ConfirmWaitlistOfferCommand(entry.Id, entry.ManagementToken.Value), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.WaitlistEntry.CannotConvert);
    }

    [Fact]
    public async Task Handle_OfferedSlotTakenInTheMeantime_ReturnsSlotNotAvailable()
    {
        var f = await SeedOrgAsync();
        var slot = TimeSlot.Create(DateTime.UtcNow.Date.AddDays(14).AddHours(10), DateTime.UtcNow.Date.AddDays(14).AddHours(10).AddMinutes(30)).Value;
        var entry = CreateOfferedEntry(f, slot, "+14155552692");

        var stolenByAnother = Booking.CreatePending(
            f.Organization.Id, f.Location.Id, f.Employee.Id, f.Service.Id, ClientContact.Create(PhoneNumber.Create("+14155552693").Value),
            slot, Money.Create(50m, "USD").Value, BookingSource.Public, DateTime.UtcNow).Value;
        stolenByAnother.Confirm(DateTime.UtcNow);

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.WaitlistEntries.Add(entry);
            seedContext.Bookings.Add(stolenByAnother);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new ConfirmWaitlistOfferCommandHandler(
            new WaitlistEntryRepository(dbContext), dbContext, new ClientRepository(dbContext), new BookingRepository(dbContext), new UnitOfWork(dbContext));

        var result = await handler.Handle(
            new ConfirmWaitlistOfferCommand(entry.Id, entry.ManagementToken.Value), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Booking.SlotNotAvailable);
    }
}