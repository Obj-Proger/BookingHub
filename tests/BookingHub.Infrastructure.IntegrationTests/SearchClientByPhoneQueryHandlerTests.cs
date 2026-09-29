using BookingHub.Application.Common;
using BookingHub.Application.Features.Clients.Queries.SearchClientByPhone;
using BookingHub.Infrastructure.Persistence.Repositories;

namespace BookingHub.Infrastructure.IntegrationTests;

[Collection(nameof(DatabaseCollection))]
public class SearchClientByPhoneQueryHandlerTests(PostgreSqlFixture fixture)
{
    private static WeeklyHours ClosedAllWeek() =>
        WeeklyHours.Create(Enum.GetValues<DayOfWeek>().Select(DailyHours.CreateClosed)).Value;

    [Fact]
    public async Task Handle_PhoneUnknownEntirely_ReturnsNotFound()
    {
        var organization = Organization.Create("Acme", $"acme-{Guid.CreateVersion7()}").Value;

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new SearchClientByPhoneQueryHandler(new ClientRepository(dbContext), dbContext);

        var result = await handler.Handle(new SearchClientByPhoneQuery(organization.Id, "+14155552670"), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Client.NotFound);
    }

    [Fact]
    public async Task Handle_ClientExistsButOnlyVisitedAnotherOrganization_ReturnsSameNotFoundAsUnknownClient()
    {
        var phone = PhoneNumber.Create("+14155552671").Value;
        var client = Client.Create(phone, "Jane Doe");
        var thisOrganization = Organization.Create("This Org", $"this-org-{Guid.CreateVersion7()}").Value;
        var otherOrganization = Organization.Create("Other Org", $"other-org-{Guid.CreateVersion7()}").Value;
        var otherLocation = Location.Create(otherOrganization.Id, "Branch", Address.Create("1 First St").Value, "UTC", ClosedAllWeek()).Value;
        var otherEmployee = Employee.Create(otherOrganization.Id, "John Smith").Value;
        var otherService = Service.Create(
            otherOrganization.Id, "Haircut", TimeSpan.FromMinutes(30), Money.Create(50m, "USD").Value, TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;
        var bookingAtOtherOrg = Booking.CreatePending(
            otherOrganization.Id, otherLocation.Id, otherEmployee.Id, otherService.Id, ClientContact.Create(phone),
            TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddMinutes(30)).Value,
            Money.Create(50m, "USD").Value, BookingSource.Public, DateTime.UtcNow).Value;
        bookingAtOtherOrg.LinkClient(client.Id);

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.AddRange(thisOrganization, otherOrganization);
            seedContext.Locations.Add(otherLocation);
            seedContext.Employees.Add(otherEmployee);
            seedContext.Services.Add(otherService);
            seedContext.Clients.Add(client);
            seedContext.Bookings.Add(bookingAtOtherOrg);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new SearchClientByPhoneQueryHandler(new ClientRepository(dbContext), dbContext);

        var result = await handler.Handle(new SearchClientByPhoneQuery(thisOrganization.Id, phone.Value), TestContext.Current.CancellationToken);

        // Same ApplicationErrors.Client.NotFound as the "phone unknown entirely" case above —
        // that's the point: this must not be distinguishable from the client not existing at all.
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Client.NotFound);
    }

    [Fact]
    public async Task Handle_ClientHasVisitedThisOrganization_ReturnsClient()
    {
        var phone = PhoneNumber.Create("+14155552672").Value;
        var client = Client.Create(phone, "Jane Doe");
        var organization = Organization.Create("Acme", $"acme-{Guid.CreateVersion7()}").Value;
        var location = Location.Create(organization.Id, "Branch", Address.Create("1 First St").Value, "UTC", ClosedAllWeek()).Value;
        var employee = Employee.Create(organization.Id, "John Smith").Value;
        var service = Service.Create(
            organization.Id, "Haircut", TimeSpan.FromMinutes(30), Money.Create(50m, "USD").Value, TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;
        var booking = Booking.CreatePending(
            organization.Id, location.Id, employee.Id, service.Id, ClientContact.Create(phone),
            TimeSlot.Create(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddMinutes(30)).Value,
            Money.Create(50m, "USD").Value, BookingSource.Public, DateTime.UtcNow).Value;
        booking.LinkClient(client.Id);

        await using (var seedContext = fixture.CreateDbContext())
        {
            seedContext.Organizations.Add(organization);
            seedContext.Locations.Add(location);
            seedContext.Employees.Add(employee);
            seedContext.Services.Add(service);
            seedContext.Clients.Add(client);
            seedContext.Bookings.Add(booking);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var dbContext = fixture.CreateDbContext();
        var handler = new SearchClientByPhoneQueryHandler(new ClientRepository(dbContext), dbContext);

        var result = await handler.Handle(new SearchClientByPhoneQuery(organization.Id, phone.Value), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.ClientId.Should().Be(client.Id);
        result.Value.Name.Should().Be("Jane Doe");
    }
}