using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Clients.Queries.GetClientProfile;
using BookingHub.Application.Features.Clients.DTOs;
using BookingHub.Infrastructure.IntegrationTests.TestDoubles;
using BookingHub.Infrastructure.Persistence.Repositories;

namespace BookingHub.Infrastructure.IntegrationTests;

[Collection(nameof(DatabaseCollection))]
public class GetClientProfileQueryHandlerTests(PostgreSqlFixture fixture)
{
    private static WeeklyHours ClosedAllWeek() =>
        WeeklyHours.Create(Enum.GetValues<DayOfWeek>().Select(DailyHours.CreateClosed)).Value;

    private static Booking CreateCompletedBooking(
        Guid organizationId, Guid locationId, Guid employeeId, Guid serviceId, PhoneNumber phone, decimal amount, string currency)
    {
        var booking = Booking.CreatePending(
            organizationId, locationId, employeeId, serviceId, ClientContact.Create(phone),
            TimeSlot.Create(DateTime.UtcNow.AddHours(-3), DateTime.UtcNow.AddHours(-2)).Value,
            Money.Create(amount, currency).Value, BookingSource.Public, DateTime.UtcNow.AddHours(-4)).Value;
        booking.Confirm(DateTime.UtcNow.AddHours(-4));
        booking.TransitionToAwaitingReview(DateTime.UtcNow);
        booking.Complete(DateTime.UtcNow);
        return booking;
    }

    [Fact]
    public async Task Handle_CompletedVisitsInDifferentCurrencies_AggregatesRevenueSeparatelyPerCurrency()
    {
        var phone = PhoneNumber.Create("+14155552673").Value;
        var client = Client.Create(phone, "Jane Doe");
        var organization = Organization.Create("Acme", $"acme-{Guid.CreateVersion7()}").Value;
        var location = Location.Create(organization.Id, "Branch", Address.Create("1 First St").Value, "UTC", ClosedAllWeek()).Value;
        var employee = Employee.Create(organization.Id, "John Smith").Value;
        var service = Service.Create(
            organization.Id, "Haircut", TimeSpan.FromMinutes(30), Money.Create(50m, "USD").Value, TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;
        var owner = OrganizationMember.Create(organization.Id, Guid.CreateVersion7(), OrganizationRole.Owner).Value;

        var usdBooking = CreateCompletedBooking(organization.Id, location.Id, employee.Id, service.Id, phone, 100m, "USD");
        var eurBooking = CreateCompletedBooking(organization.Id, location.Id, employee.Id, service.Id, phone, 40m, "EUR");
        usdBooking.LinkClient(client.Id);
        eurBooking.LinkClient(client.Id);

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.Locations.Add(location);
            seedContext.Employees.Add(employee);
            seedContext.Services.Add(service);
            seedContext.Clients.Add(client);
            seedContext.OrganizationMembers.Add(owner);
            seedContext.Bookings.AddRange(usdBooking, eurBooking);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new GetClientProfileQueryHandler(
            new ClientRepository(dbContext), dbContext, new FixedCurrentUser(owner.UserId), new OrganizationMemberRepository(dbContext));

        var result = await handler.Handle(new GetClientProfileQuery(organization.Id, client.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.CompletedVisits.Should().Be(2);
        result.Value.TotalRevenue.Should().BeEquivalentTo(
        [
            new RevenueByCurrency("USD", 100m),
            new RevenueByCurrency("EUR", 40m)
        ]);
        result.Value.AverageCheck.Should().BeEquivalentTo(
        [
            new RevenueByCurrency("USD", 100m),
            new RevenueByCurrency("EUR", 40m)
        ]);
    }

    [Fact]
    public async Task Handle_CallerIsEmployee_SeesOnlyOwnVisitsWithThisClient()
    {
        var phone = PhoneNumber.Create("+14155552674").Value;
        var client = Client.Create(phone, "Jane Doe");
        var organization = Organization.Create("Acme", $"acme-{Guid.CreateVersion7()}").Value;
        var location = Location.Create(organization.Id, "Branch", Address.Create("1 First St").Value, "UTC", ClosedAllWeek()).Value;
        var employeeA = Employee.Create(organization.Id, "Employee A").Value;
        var employeeB = Employee.Create(organization.Id, "Employee B").Value;
        var service = Service.Create(
            organization.Id, "Haircut", TimeSpan.FromMinutes(30), Money.Create(50m, "USD").Value, TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;
        var callerUserId = Guid.CreateVersion7();
        var callerAsEmployeeA = OrganizationMember.Create(organization.Id, callerUserId, OrganizationRole.Employee, employeeId: employeeA.Id).Value;

        var bookingWithEmployeeA = CreateCompletedBooking(organization.Id, location.Id, employeeA.Id, service.Id, phone, 50m, "USD");
        var bookingWithEmployeeB = CreateCompletedBooking(organization.Id, location.Id, employeeB.Id, service.Id, phone, 50m, "USD");
        bookingWithEmployeeA.LinkClient(client.Id);
        bookingWithEmployeeB.LinkClient(client.Id);

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.Locations.Add(location);
            seedContext.Employees.AddRange(employeeA, employeeB);
            seedContext.Services.Add(service);
            seedContext.Clients.Add(client);
            seedContext.OrganizationMembers.Add(callerAsEmployeeA);
            seedContext.Bookings.AddRange(bookingWithEmployeeA, bookingWithEmployeeB);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new GetClientProfileQueryHandler(
            new ClientRepository(dbContext), dbContext, new FixedCurrentUser(callerUserId), new OrganizationMemberRepository(dbContext));

        var result = await handler.Handle(new GetClientProfileQuery(organization.Id, client.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalVisits.Should().Be(1);
        result.Value.Visits.Single().BookingId.Should().Be(bookingWithEmployeeA.Id);
    }
}