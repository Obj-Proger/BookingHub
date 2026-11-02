using BookingHub.Application.Common.Notifications;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;
using BookingHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingHub.Infrastructure.Persistence.Seeding;

/// <summary>
/// Fills an empty development database with one demo organization — two locations in different
/// time zones, three employees with weekly schedules, services, staff accounts for every role,
/// clients, bookings in every status, reviews and a waitlist entry — so the Web app can be tried
/// by hand straight away. Run with <c>dotnet run --project src/BookingHub.API -- seed</c>.
/// Dates are relative to today, so the data is always "current" when it is seeded.
/// </summary>
public static class DevelopmentSeeder
{
    public const string OrganizationSlug = "demo";

    /// <summary>The password of every seeded account — development data only.</summary>
    public const string Password = "Demo1234";

    /// <returns>False when the database already had data and nothing was seeded.</returns>
    public static async Task<bool> SeedAsync(IServiceProvider services, TextWriter output, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var dbContext = provider.GetRequiredService<ApplicationDbContext>();

        if (await dbContext.Organizations.AnyAsync(cancellationToken))
        {
            await output.WriteLineAsync("The database already has data; nothing was seeded. Seeding needs an empty database.");
            return false;
        }

        var utcNow = provider.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Accounts first: UserManager saves on its own (inside this transaction).
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var ownerUser = await CreateUserAsync(userManager, "owner@demo.test");
        var adminUser = await CreateUserAsync(userManager, "admin@demo.test");
        var managerUser = await CreateUserAsync(userManager, "manager@demo.test");
        var employeeUser = await CreateUserAsync(userManager, "anna@demo.test");

        var organization = Organization.Create("Demo Salon", OrganizationSlug).Value;
        dbContext.Organizations.Add(organization);

        // Every other row belongs to the organization: row-level security only accepts them inside its scope.
        await provider.GetRequiredService<RowLevelSecuritySession>().EnterAsync(organization.Id, cancellationToken);

        var data = new Builder(dbContext, organization.Id, utcNow);
        data.Build(ownerUser, adminUser, managerUser, employeeUser);

        // Seeded history is not news: nobody is notified and no waitlist offer is made for it.
        foreach (var entry in dbContext.ChangeTracker.Entries<IHasDomainEvents>())
            entry.Entity.ClearDomainEvents();

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await WriteSummaryAsync(output, provider.GetRequiredService<IWebLinkBuilder>(), data);
        return true;
    }

    private static async Task<ApplicationUser> CreateUserAsync(UserManager<ApplicationUser> userManager, string email)
    {
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, Password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Seeding {email} failed: {string.Join(" ", result.Errors.Select(e => e.Description))}");

        return user;
    }

    private static async Task WriteSummaryAsync(TextWriter output, IWebLinkBuilder links, Builder data)
    {
        await output.WriteLineAsync($"""
            Seeded the "{OrganizationSlug}" organization.

            Staff sign-in ({links.Build("/login")}), password {Password} for every account:
              owner@demo.test    Owner
              admin@demo.test    Administrator
              manager@demo.test  Location manager (Central)
              anna@demo.test     Employee (Anna Smirnova)

            Public booking page: {links.Build($"/book/{OrganizationSlug}")}

            Guest links (normally texted to the guest):
            """);

        foreach (var (label, path) in data.GuestLinks)
            await output.WriteLineAsync($"  {label}:\n    {links.Build(path)}");
    }

    /// <summary>Creates the entities and adds them to the context; nothing is saved here.</summary>
    private sealed class Builder(ApplicationDbContext dbContext, Guid organizationId, DateTime utcNow)
    {
        private const string Currency = "RUB";

        private const string CentralZoneId = "Asia/Yekaterinburg";
        private const string RiversideZoneId = "Europe/Moscow";

        private static readonly TimeZoneInfo CentralZone = TimeZoneInfo.FindSystemTimeZoneById(CentralZoneId);
        private static readonly TimeZoneInfo RiversideZone = TimeZoneInfo.FindSystemTimeZoneById(RiversideZoneId);

        public List<(string Label, string Path)> GuestLinks { get; } = [];

        public void Build(ApplicationUser owner, ApplicationUser admin, ApplicationUser manager, ApplicationUser employeeAccount)
        {
            var central = AddLocation("Central", "1 Lenin Street, Yekaterinburg", CentralZoneId,
                Open(DayOfWeek.Monday, DayOfWeek.Saturday, 9, 20));
            var riverside = AddLocation("Riverside", "15 Embankment, Moscow", RiversideZoneId,
                Open(DayOfWeek.Monday, DayOfWeek.Friday, 10, 19));

            var haircut = AddService("Haircut", minutes: 45, price: 1500, bufferAfterMinutes: 0, "#4F46E5");
            var colouring = AddService("Colouring", minutes: 120, price: 5000, bufferAfterMinutes: 15, "#DB2777");
            var beardTrim = AddService("Beard trim", minutes: 30, price: 800, bufferAfterMinutes: 0, "#059669");
            dbContext.LocationServiceOverrides.Add(
                LocationServiceOverride.Create(riverside.Id, haircut.Id, Money.Create(1800, Currency).Value).Value);

            var anna = AddEmployee("Anna Smirnova");
            var boris = AddEmployee("Boris Volkov");
            var clara = AddEmployee("Clara Novak");
            anna.LinkUser(employeeAccount.Id).ThrowIfFailure();

            var annaCentral = Assign(anna, central, Weekdays, 9, 17);
            Assign(boris, central, [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday], 12, 20);
            Assign(boris, riverside, [DayOfWeek.Tuesday, DayOfWeek.Thursday], 10, 19);
            Assign(clara, riverside, Weekdays, 10, 19);

            var centralToday = Today(CentralZone);
            var riversideToday = Today(RiversideZone);

            // Anna takes a day off three weekdays from now: no slots that day.
            dbContext.ScheduleExceptions.Add(ScheduleException.CreateDayOff(annaCentral.Id, Weekday(centralToday, 3)).Value);

            AddMember(owner, OrganizationRole.Owner);
            AddMember(admin, OrganizationRole.Administrator);
            AddMember(manager, OrganizationRole.LocationManager, locationId: central.Id);
            AddMember(employeeAccount, OrganizationRole.Employee, employeeId: anna.Id);

            var ivan = AddClient("+79001000001", "Ivan Petrov", "ivan@example.test");
            var maria = AddClient("+79001000002", "Maria Ivanova", null);
            var oleg = AddClient("+79001000003", "Oleg Sokolov", null);
            var elena = AddClient("+79001000004", "Elena Kuznetsova", null);

            // History: completed visits (with reviews, one hidden by moderation), a no-show, and a
            // visit that has ended but is not resolved yet (staff can mark it completed or no-show).
            var past1 = AddPastBooking(central, anna, haircut, ivan, Weekday(centralToday, -1), 10, BookingStatus.Completed);
            var past2 = AddPastBooking(central, anna, colouring, maria, Weekday(centralToday, -2), 13, BookingStatus.Completed);
            var past3 = AddPastBooking(riverside, clara, beardTrim, oleg, Weekday(riversideToday, -1), 11, BookingStatus.Completed);
            AddPastBooking(riverside, clara, haircut, elena, Weekday(riversideToday, -3), 16, BookingStatus.NoShow);
            var awaitingReview = AddPastBooking(central, anna, beardTrim, oleg, Weekday(centralToday, -1), 15, BookingStatus.AwaitingReview);

            AddReview(past1, 5, "Great haircut, will come back.");
            AddReview(past2, 4, "Lovely colour, took a bit longer than expected.");
            AddReview(past3, 2, "Rude remarks about my beard.").Hide();

            // Upcoming: confirmed, pending (waiting for the guest to confirm) and cancelled. Anna's
            // confirmed bookings are two working days out, beyond the 24-hour cancellation deadline,
            // so the guest can still cancel or reschedule them.
            var annaDay = Weekday(centralToday, 2);
            var annaMorning = AddUpcomingBooking(central, anna, haircut, ivan, annaDay, 10, BookingStatus.Confirmed);
            AddUpcomingBooking(central, anna, colouring, maria, annaDay, 11, BookingStatus.Confirmed);
            AddUpcomingBooking(riverside, clara, beardTrim, elena, Weekday(riversideToday, 1), 15, BookingStatus.Confirmed);
            var pending = AddUpcomingBooking(central, anna, haircut, elena, Weekday(centralToday, 1), 14, BookingStatus.Pending);
            AddUpcomingBooking(riverside, clara, haircut, oleg, Weekday(riversideToday, 4), 12, BookingStatus.Cancelled);

            // Someone waits for any free time with Anna that day: cancelling her 10:00 booking offers it to them.
            var waitlistContact = ClientContact.Create("+79001000005", "Pavel Orlov", null).Value;
            dbContext.WaitlistEntries.Add(WaitlistEntry.Create(
                organizationId, central.Id, anna.Id, haircut.Id, waitlistContact,
                TimeSlot.Create(Utc(annaDay, 9, CentralZone), Utc(annaDay, 17, CentralZone)).Value, utcNow).Value);

            GuestLinks.Add(("Manage Ivan's confirmed haircut with Anna (cancel it to trigger the waitlist offer)",
                $"/book/{OrganizationSlug}/manage/{annaMorning.Id}?token={annaMorning.CancellationToken.Value}"));
            GuestLinks.Add(("Confirm Elena's pending haircut (within 30 minutes of seeding, then it expires)",
                $"/book/{OrganizationSlug}/confirm/{pending.Id}?token={pending.ConfirmationToken.Value}"));
            GuestLinks.Add(("Review Oleg's visit with Anna yesterday (works once staff mark it completed)",
                $"/book/{OrganizationSlug}/review/{awaitingReview.Id}?token={awaitingReview.CancellationToken.Value}"));
        }

        private static readonly DayOfWeek[] Weekdays =
            [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];

        private Location AddLocation(string name, string address, string timeZoneId, WeeklyHours hours)
        {
            var location = Location.Create(organizationId, name, Address.Create(address).Value, timeZoneId, hours).Value;
            dbContext.Locations.Add(location);
            return location;
        }

        private static WeeklyHours Open(DayOfWeek first, DayOfWeek last, int openHour, int closeHour) =>
            WeeklyHours.Create(Enum.GetValues<DayOfWeek>().Select(day =>
                IsBetween(day, first, last)
                    ? DailyHours.CreateOpen(day, new TimeOnly(openHour, 0), new TimeOnly(closeHour, 0)).Value
                    : DailyHours.CreateClosed(day))).Value;

        /// <summary>Monday-first ordering, so Monday..Saturday includes Saturday.</summary>
        private static bool IsBetween(DayOfWeek day, DayOfWeek first, DayOfWeek last)
        {
            static int Index(DayOfWeek d) => ((int)d + 6) % 7;
            return Index(day) >= Index(first) && Index(day) <= Index(last);
        }

        private Service AddService(string name, int minutes, decimal price, int bufferAfterMinutes, string color)
        {
            var service = Service.Create(
                organizationId, name, TimeSpan.FromMinutes(minutes), Money.Create(price, Currency).Value,
                TimeSpan.Zero, TimeSpan.FromMinutes(bufferAfterMinutes), color).Value;
            dbContext.Services.Add(service);
            return service;
        }

        private Employee AddEmployee(string fullName)
        {
            var employee = Employee.Create(organizationId, fullName).Value;
            dbContext.Employees.Add(employee);
            return employee;
        }

        private EmployeeLocationAssignment Assign(
            Employee employee, Location location, IEnumerable<DayOfWeek> days, int startHour, int endHour)
        {
            var assignment = EmployeeLocationAssignment.Create(employee.Id, location.Id).Value;
            dbContext.EmployeeLocationAssignments.Add(assignment);

            foreach (var day in days)
            {
                dbContext.RecurringSchedules.Add(RecurringSchedule.Create(
                    assignment.Id, day, new TimeOnly(startHour, 0), new TimeOnly(endHour, 0)).Value);
            }

            return assignment;
        }

        private void AddMember(ApplicationUser user, OrganizationRole role, Guid? locationId = null, Guid? employeeId = null) =>
            dbContext.OrganizationMembers.Add(OrganizationMember.Create(organizationId, user.Id, role, locationId, employeeId).Value);

        private Client AddClient(string phone, string name, string? email)
        {
            var client = Client.Create(ClientContact.Create(phone, name, email).Value);
            dbContext.Clients.Add(client);
            return client;
        }

        private Booking AddPastBooking(
            Location location, Employee employee, Service service, Client client, DateOnly date, int hour, BookingStatus status)
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(location.TimeZone);
            var slot = Slot(date, hour, service, zone);

            // Made, confirmed and attended in the past: the domain is given the times those steps happened.
            var bookedAt = slot.StartUtc.AddDays(-3);
            var booking = NewBooking(location, employee, service, client, slot, BookingSource.StaffCreated, bookedAt);
            booking.Confirm(bookedAt).ThrowIfFailure();
            booking.TransitionToAwaitingReview(slot.EndUtc.AddMinutes(30)).ThrowIfFailure();

            switch (status)
            {
                case BookingStatus.Completed:
                    booking.Complete(slot.EndUtc.AddHours(1)).ThrowIfFailure();
                    break;
                case BookingStatus.NoShow:
                    booking.MarkNoShow(slot.EndUtc.AddHours(1)).ThrowIfFailure();
                    break;
            }

            return booking;
        }

        private Booking AddUpcomingBooking(
            Location location, Employee employee, Service service, Client client, DateOnly date, int hour, BookingStatus status)
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(location.TimeZone);
            var booking = NewBooking(location, employee, service, client, Slot(date, hour, service, zone), BookingSource.Public, utcNow);

            if (status is BookingStatus.Confirmed or BookingStatus.Cancelled)
                booking.Confirm(utcNow).ThrowIfFailure();
            if (status == BookingStatus.Cancelled)
                booking.Cancel("Plans changed.", utcNow).ThrowIfFailure();

            return booking;
        }

        private Booking NewBooking(
            Location location, Employee employee, Service service, Client client, TimeSlot slot, BookingSource source, DateTime bookedAtUtc)
        {
            var contact = ClientContact.Create(client.Phone, client.Name, client.Email).Value;
            var booking = Booking.CreatePending(
                organizationId, location.Id, employee.Id, service.Id, contact, slot, service.BasePrice, source, bookedAtUtc).Value;
            booking.LinkClient(client.Id).ThrowIfFailure();
            dbContext.Bookings.Add(booking);
            return booking;
        }

        private Review AddReview(Booking booking, int rating, string comment)
        {
            var review = Review.Create(organizationId, booking.LocationId, booking.EmployeeId, booking.Id, rating, comment, utcNow).Value;
            dbContext.Reviews.Add(review);
            return review;
        }

        private static TimeSlot Slot(DateOnly date, int hour, Service service, TimeZoneInfo zone)
        {
            var start = Utc(date, hour, zone);
            return TimeSlot.Create(start, start + service.Duration).Value;
        }

        private static DateTime Utc(DateOnly localDate, int hour, TimeZoneInfo zone) =>
            TimeZoneInfo.ConvertTimeToUtc(localDate.ToDateTime(new TimeOnly(hour, 0)), zone);

        private DateOnly Today(TimeZoneInfo zone) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcNow, zone));

        /// <summary>The weekday <paramref name="offset"/> working days after (or, if negative, before) <paramref name="from"/>.</summary>
        private static DateOnly Weekday(DateOnly from, int offset)
        {
            var date = from;
            var remaining = Math.Abs(offset);
            while (remaining > 0)
            {
                date = date.AddDays(Math.Sign(offset));
                if (date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
                    remaining--;
            }

            return date;
        }
    }
}

file static class ResultExtensions
{
    /// <summary>Seed data is valid by construction; a failure here is a bug in the seeder.</summary>
    public static void ThrowIfFailure(this Result result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException($"Seeding failed: [{result.Error.Code}] {result.Error.Message}");
    }
}