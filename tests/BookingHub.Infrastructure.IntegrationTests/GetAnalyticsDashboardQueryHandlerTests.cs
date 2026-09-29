using BookingHub.Application.Common;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Analytics.Queries.GetAnalyticsDashboard;
using BookingHub.Infrastructure.IntegrationTests.TestDoubles;
using BookingHub.Infrastructure.Persistence.Repositories;

namespace BookingHub.Infrastructure.IntegrationTests;

[Collection(nameof(DatabaseCollection))]
public class GetAnalyticsDashboardQueryHandlerTests(PostgreSqlFixture fixture)
{
    private static WeeklyHours ClosedAllWeek() =>
        WeeklyHours.Create(Enum.GetValues<DayOfWeek>().Select(DailyHours.CreateClosed)).Value;

    private static Booking CreateCompletedBooking(
        Guid organizationId, Guid locationId, Guid employeeId, Guid serviceId, DateTime slotStartUtc, decimal amount)
    {
        var booking = Booking.CreatePending(
            organizationId, locationId, employeeId, serviceId, ClientContact.Create(PhoneNumber.Create("+14155552671").Value),
            TimeSlot.Create(slotStartUtc, slotStartUtc.AddMinutes(30)).Value,
            Money.Create(amount, "USD").Value, BookingSource.Public, slotStartUtc.AddHours(-1)).Value;
        booking.Confirm(slotStartUtc.AddHours(-1));
        booking.TransitionToAwaitingReview(slotStartUtc.AddMinutes(31));
        booking.Complete(slotStartUtc.AddMinutes(31));
        return booking;
    }

    [Fact]
    public async Task Handle_LocationIdIsEmptySentinel_AggregatesAcrossAllLocations()
    {
        var organization = Organization.Create("Acme", $"acme-{Guid.CreateVersion7()}").Value;
        var locationA = Location.Create(organization.Id, "Branch A", Address.Create("1 First St").Value, "UTC", ClosedAllWeek()).Value;
        var locationB = Location.Create(organization.Id, "Branch B", Address.Create("2 Second St").Value, "UTC", ClosedAllWeek()).Value;
        var employee = Employee.Create(organization.Id, "John Smith").Value;
        var service = Service.Create(
            organization.Id, "Haircut", TimeSpan.FromMinutes(30), Money.Create(50m, "USD").Value, TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;
        var owner = OrganizationMember.Create(organization.Id, Guid.CreateVersion7(), OrganizationRole.Owner).Value;

        var slotStart = DateTime.UtcNow.AddDays(-1);
        var bookingAtA = CreateCompletedBooking(organization.Id, locationA.Id, employee.Id, service.Id, slotStart, 50m);
        var bookingAtB = CreateCompletedBooking(organization.Id, locationB.Id, employee.Id, service.Id, slotStart, 30m);

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.Locations.AddRange(locationA, locationB);
            seedContext.Employees.Add(employee);
            seedContext.Services.Add(service);
            seedContext.OrganizationMembers.Add(owner);
            seedContext.Bookings.AddRange(bookingAtA, bookingAtB);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new GetAnalyticsDashboardQueryHandler(dbContext, new FixedCurrentUser(owner.UserId), new OrganizationMemberRepository(dbContext));

        var networkWide = await handler.Handle(
            new GetAnalyticsDashboardQuery(organization.Id, Guid.Empty, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow),
            TestContext.Current.CancellationToken);
        var locationAOnly = await handler.Handle(
            new GetAnalyticsDashboardQuery(organization.Id, locationA.Id, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow),
            TestContext.Current.CancellationToken);

        networkWide.Value.TotalRevenue.Single().Amount.Should().Be(80m);
        locationAOnly.Value.TotalRevenue.Single().Amount.Should().Be(50m);
    }

    [Fact]
    public async Task Handle_AdministratorWithoutFinancialAccess_ReturnsForbidden()
    {
        var organization = Organization.Create("Acme", $"acme-{Guid.CreateVersion7()}").Value;
        var administrator = OrganizationMember.Create(organization.Id, Guid.CreateVersion7(), OrganizationRole.Administrator).Value;

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.OrganizationMembers.Add(administrator);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new GetAnalyticsDashboardQueryHandler(dbContext, new FixedCurrentUser(administrator.UserId), new OrganizationMemberRepository(dbContext));

        var result = await handler.Handle(
            new GetAnalyticsDashboardQuery(organization.Id, Guid.Empty, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Authorization.FinancialAccessDisabled);
    }

    [Fact]
    public async Task Handle_AdministratorWithFinancialAccessEnabled_Succeeds()
    {
        var organization = Organization.Create("Acme", $"acme-{Guid.CreateVersion7()}").Value;
        organization.SetAdministratorFinancialAccess(true);
        var administrator = OrganizationMember.Create(organization.Id, Guid.CreateVersion7(), OrganizationRole.Administrator).Value;

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.OrganizationMembers.Add(administrator);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new GetAnalyticsDashboardQueryHandler(dbContext, new FixedCurrentUser(administrator.UserId), new OrganizationMemberRepository(dbContext));

        var result = await handler.Handle(
            new GetAnalyticsDashboardQuery(organization.Id, Guid.Empty, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_BookingOutsideDateRange_IsExcluded()
    {
        var organization = Organization.Create("Acme", $"acme-{Guid.CreateVersion7()}").Value;
        var location = Location.Create(organization.Id, "Branch", Address.Create("1 First St").Value, "UTC", ClosedAllWeek()).Value;
        var employee = Employee.Create(organization.Id, "John Smith").Value;
        var service = Service.Create(
            organization.Id, "Haircut", TimeSpan.FromMinutes(30), Money.Create(50m, "USD").Value, TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;
        var owner = OrganizationMember.Create(organization.Id, Guid.CreateVersion7(), OrganizationRole.Owner).Value;

        var insideRange = CreateCompletedBooking(organization.Id, location.Id, employee.Id, service.Id, DateTime.UtcNow.AddDays(-5), 50m);
        var outsideRange = CreateCompletedBooking(organization.Id, location.Id, employee.Id, service.Id, DateTime.UtcNow.AddDays(-20), 999m);

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.Locations.Add(location);
            seedContext.Employees.Add(employee);
            seedContext.Services.Add(service);
            seedContext.OrganizationMembers.Add(owner);
            seedContext.Bookings.AddRange(insideRange, outsideRange);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new GetAnalyticsDashboardQueryHandler(dbContext, new FixedCurrentUser(owner.UserId), new OrganizationMemberRepository(dbContext));

        var result = await handler.Handle(
            new GetAnalyticsDashboardQuery(organization.Id, Guid.Empty, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow),
            TestContext.Current.CancellationToken);

        result.Value.TotalRevenue.Single().Amount.Should().Be(50m);
    }
}