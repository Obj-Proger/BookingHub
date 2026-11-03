using System.Globalization;
using System.Net;
using BookingHub.API.Controllers.Employees;
using BookingHub.API.Controllers.Locations;
using BookingHub.API.Controllers.Organizations;
using BookingHub.API.Controllers.Services;
using BookingHub.Application.Features.Bookings.DTOs;
using BookingHub.Application.Features.Employees.DTOs;
using BookingHub.Application.Features.Locations.DTOs;
using BookingHub.Application.Features.Organizations.DTOs;
using BookingHub.Application.Features.Public.DTOs;
using BookingHub.Application.Features.Reviews.DTOs;
using BookingHub.Application.Features.Services.DTOs;
using BookingHub.Domain.Enums;
using BookingHub.Infrastructure.Persistence.Seeding;

namespace BookingHub.API.Tests;

/// <summary>
/// What staff do in the back office, end to end through the API on the demo data — the catalogue, the
/// team and its schedules, locations, settings, members, reviews and visits — and what a guest sees
/// of it on the public pages afterwards.
/// </summary>
public class StaffFlowTests(SeededApiFixture fixture) : IClassFixture<SeededApiFixture>
{
    private const string RiversideZone = "Europe/Moscow";

    private static readonly string PublicApi = $"api/v1/public/{DevelopmentSeeder.OrganizationSlug}";

    private DemoData Demo => fixture.Demo;

    private string OrganizationApi => $"api/v1/organizations/{Demo.OrganizationId}";

    [Fact]
    public async Task NewService_EditedAndPricedPerLocation_ShowsInThePublicCatalogue()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.Factory.CreateClient();
        var owner = await SignInAsync(client, DemoData.OwnerEmail, cancellationToken);
        var guest = new TestApi(client);

        var service = await owner.PostAsync<ServiceCreatedResponse>($"{OrganizationApi}/services",
            new CreateServiceRequest("Manicure", TimeSpan.FromMinutes(60), 2000m, "RUB", TimeSpan.Zero, TimeSpan.FromMinutes(10), "#22AA88"),
            cancellationToken);
        var servicePath = $"{OrganizationApi}/services/{service.ServiceId}";

        await owner.PutAsync($"{servicePath}/name", new RenameServiceRequest("Classic manicure"), cancellationToken);
        await owner.PutAsync($"{servicePath}/pricing", new UpdateServicePricingRequest(2200m, "RUB"), cancellationToken);
        await owner.PutAsync($"{servicePath}/duration", new UpdateServiceDurationRequest(TimeSpan.FromMinutes(50)), cancellationToken);
        await owner.PutAsync($"{servicePath}/buffers",
            new UpdateServiceBuffersRequest(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10)), cancellationToken);
        await owner.PutAsync($"{servicePath}/color", new UpdateServiceColorRequest("#118866"), cancellationToken);

        (await owner.GetAsync<ServiceResponse>(servicePath, cancellationToken)).Should().Be(new ServiceResponse(
            service.ServiceId, "Classic manicure", TimeSpan.FromMinutes(50), 2200m, "RUB",
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10), "#118866"));

        var overridesPath = $"{OrganizationApi}/locations/{Demo.RiversideId}/service-overrides";
        var priceOverride = await owner.PostAsync<LocationServiceOverrideCreatedResponse>(overridesPath,
            new CreateLocationServiceOverrideRequest(service.ServiceId, 2500m, "RUB"), cancellationToken);
        await owner.PutAsync($"{overridesPath}/{priceOverride.OverrideId}",
            new UpdateLocationServiceOverridePriceRequest(2600m, "RUB"), cancellationToken);

        (await PublicServiceAsync(guest, Demo.RiversideId, service.ServiceId, cancellationToken))
            .Should().Be(new PublicServiceResponse(service.ServiceId, "Classic manicure", TimeSpan.FromMinutes(50), 2600m, "RUB"));
        (await PublicServiceAsync(guest, Demo.CentralId, service.ServiceId, cancellationToken)).PriceAmount.Should().Be(2200m);

        await owner.DeleteAsync($"{overridesPath}/{priceOverride.OverrideId}", cancellationToken);
        (await PublicServiceAsync(guest, Demo.RiversideId, service.ServiceId, cancellationToken)).PriceAmount.Should().Be(2200m);
    }

    [Fact]
    public async Task NewEmployee_AssignedAndScheduled_GuestsSeeTheirTimes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.Factory.CreateClient();
        var owner = await SignInAsync(client, DemoData.OwnerEmail, cancellationToken);
        var guest = new TestApi(client);

        var employee = await owner.PostAsync<EmployeeCreatedResponse>($"{OrganizationApi}/employees",
            new CreateEmployeeRequest("Dmitry Orlov"), cancellationToken);
        var employeePath = $"{OrganizationApi}/employees/{employee.EmployeeId}";
        await owner.PutAsync($"{employeePath}/name", new RenameEmployeeRequest("Dmitri Orlov"), cancellationToken);
        await owner.PutAsync($"{employeePath}/photo", new UpdateEmployeePhotoRequest("https://example.com/dmitri.jpg"), cancellationToken);
        (await owner.GetAsync<EmployeeResponse>(employeePath, cancellationToken)).Should().Be(
            new EmployeeResponse(employee.EmployeeId, "Dmitri Orlov", "https://example.com/dmitri.jpg", IsBookable: true));

        var locationPath = $"{OrganizationApi}/locations/{Demo.RiversideId}";
        var assignment = await owner.PostAsync<EmployeeAssignmentCreatedResponse>($"{locationPath}/employees",
            new AssignEmployeeToLocationRequest(employee.EmployeeId), cancellationToken);
        var schedulePath = $"{locationPath}/assignments/{assignment.AssignmentId}/schedule";

        // A Wednesday at least two days ahead; Riverside is open 10:00–19:00 on weekdays.
        var day = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2);
        while (day.DayOfWeek != DayOfWeek.Wednesday)
            day = day.AddDays(1);

        var shift = await owner.PostAsync<RecurringScheduleCreatedResponse>($"{schedulePath}/recurring",
            new CreateRecurringScheduleRequest(DayOfWeek.Wednesday, new TimeOnly(10, 0), new TimeOnly(14, 0)), cancellationToken);
        await owner.PutAsync($"{locationPath}/schedule/recurring/{shift.RecurringScheduleId}",
            new RescheduleRecurringScheduleRequest(new TimeOnly(11, 0), new TimeOnly(15, 0)), cancellationToken);

        (await PublicEmployeesAsync(guest, Demo.RiversideId, cancellationToken)).Should().Contain(new PublicEmployeeResponse(employee.EmployeeId, "Dmitri Orlov"));
        await ShouldOfferHaircutsAsync(guest, employee.EmployeeId, day, from: new TimeOnly(11, 0), to: new TimeOnly(15, 0), cancellationToken);

        var shorterDay = await owner.PostAsync<ScheduleExceptionCreatedResponse>($"{schedulePath}/exceptions/modified-hours",
            new CreateModifiedHoursScheduleExceptionRequest(day, new TimeOnly(12, 0), new TimeOnly(13, 0)), cancellationToken);
        await ShouldOfferHaircutsAsync(guest, employee.EmployeeId, day, from: new TimeOnly(12, 0), to: new TimeOnly(13, 0), cancellationToken);
        await owner.DeleteAsync($"{locationPath}/schedule/exceptions/{shorterDay.ScheduleExceptionId}", cancellationToken);

        var dayOff = await owner.PostAsync<ScheduleExceptionCreatedResponse>($"{schedulePath}/exceptions/day-off",
            new CreateDayOffScheduleExceptionRequest(day), cancellationToken);
        (await owner.GetAsync<AssignmentScheduleResponse>(schedulePath, cancellationToken)).Exceptions
            .Should().ContainSingle(exception => exception.ScheduleExceptionId == dayOff.ScheduleExceptionId && exception.Date == day);
        (await AvailableHaircutsAsync(guest, employee.EmployeeId, day, cancellationToken)).Should().BeEmpty();
        await owner.DeleteAsync($"{locationPath}/schedule/exceptions/{dayOff.ScheduleExceptionId}", cancellationToken);
        await ShouldOfferHaircutsAsync(guest, employee.EmployeeId, day, from: new TimeOnly(11, 0), to: new TimeOnly(15, 0), cancellationToken);

        await owner.PutAsync($"{employeePath}/bookable", new SetEmployeeBookableRequest(false), cancellationToken);
        (await PublicEmployeesAsync(guest, Demo.RiversideId, cancellationToken)).Should().NotContain(e => e.EmployeeId == employee.EmployeeId);
        await owner.PutAsync($"{employeePath}/bookable", new SetEmployeeBookableRequest(true), cancellationToken);

        await owner.DeleteAsync($"{locationPath}/schedule/recurring/{shift.RecurringScheduleId}", cancellationToken);
        (await owner.GetAsync<AssignmentScheduleResponse>(schedulePath, cancellationToken)).RecurringSchedules.Should().BeEmpty();
        await owner.DeleteAsync($"{locationPath}/employees/{assignment.AssignmentId}", cancellationToken);
        (await PublicEmployeesAsync(guest, Demo.RiversideId, cancellationToken)).Should().NotContain(e => e.EmployeeId == employee.EmployeeId);
    }

    [Fact]
    public async Task NewLocation_RenamedMovedAndReopenedOnSaturdays_IsListedForGuests()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.Factory.CreateClient();
        var owner = await SignInAsync(client, DemoData.OwnerEmail, cancellationToken);
        var guest = new TestApi(client);

        var location = await owner.PostAsync<LocationCreatedResponse>($"{OrganizationApi}/locations",
            new CreateLocationRequest("Lakeside", "5 Lake Road", "Asia/Yekaterinburg", WeekdaysNineToSix(saturday: null)),
            cancellationToken);
        var locationPath = $"{OrganizationApi}/locations/{location.LocationId}";

        var saturday = new DailyHoursDto(DayOfWeek.Saturday, new TimeOnly(10, 0), new TimeOnly(16, 0));
        await owner.PutAsync($"{locationPath}/name", new RenameLocationRequest("Lakeside Studio"), cancellationToken);
        await owner.PutAsync($"{locationPath}/address", new UpdateLocationAddressRequest("7 Lake Road"), cancellationToken);
        await owner.PutAsync($"{locationPath}/working-hours",
            new UpdateLocationWorkingHoursRequest(WeekdaysNineToSix(saturday)), cancellationToken);

        var edited = await owner.GetAsync<LocationResponse>(locationPath, cancellationToken);
        edited.Name.Should().Be("Lakeside Studio");
        edited.Address.Should().Be("7 Lake Road");
        edited.WorkingHours.Should().ContainEquivalentOf(saturday);

        (await guest.GetAsync<List<PublicLocationResponse>>($"{PublicApi}/locations", cancellationToken))
            .Should().ContainSingle(l => l.LocationId == location.LocationId && l.Name == "Lakeside Studio" && l.Address == "7 Lake Road");
    }

    [Fact]
    public async Task OrganizationSettings_Changed_AreReadBack()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.Factory.CreateClient();
        var owner = await SignInAsync(client, DemoData.OwnerEmail, cancellationToken);

        await owner.PutAsync($"{OrganizationApi}/name", new RenameOrganizationRequest("Demo Salon & Spa"), cancellationToken);
        await owner.PutAsync($"{OrganizationApi}/settings/cancellation-deadline",
            new UpdateCancellationDeadlineRequest(TimeSpan.FromHours(12)), cancellationToken);
        await owner.PutAsync($"{OrganizationApi}/settings/pending-confirmation-window",
            new UpdateSchedulingWindowRequest(TimeSpan.FromMinutes(45)), cancellationToken);
        await owner.PutAsync($"{OrganizationApi}/settings/auto-complete-window",
            new UpdateSchedulingWindowRequest(TimeSpan.FromHours(6)), cancellationToken);
        await owner.PutAsync($"{OrganizationApi}/settings/waitlist-offer-window",
            new UpdateSchedulingWindowRequest(TimeSpan.FromHours(1)), cancellationToken);
        await owner.PutAsync($"{OrganizationApi}/settings/administrator-financial-access",
            new SetAdministratorFinancialAccessRequest(true), cancellationToken);

        (await owner.GetAsync<OrganizationResponse>(OrganizationApi, cancellationToken)).Should().Be(new OrganizationResponse(
            Demo.OrganizationId, "Demo Salon & Spa", DevelopmentSeeder.OrganizationSlug,
            CancellationDeadline: TimeSpan.FromHours(12), PendingConfirmationWindow: TimeSpan.FromMinutes(45),
            AutoCompleteWindow: TimeSpan.FromHours(6), WaitlistOfferWindow: TimeSpan.FromHours(1),
            CanAdministratorsViewFinancials: true));
    }

    [Fact]
    public async Task Member_AddedByEmail_ChangedToLocationManager_AndRemoved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.Factory.CreateClient();
        var owner = await SignInAsync(client, DemoData.OwnerEmail, cancellationToken);
        const string Email = "new.colleague@demo.test";
        var colleague = new TestApi(client, await TestHelpers.RegisterAndLoginAsync(client, Email));

        var member = await owner.PostAsync<OrganizationMemberCreatedResponse>($"{OrganizationApi}/members/by-email",
            new AddOrganizationMemberByEmailRequest(Email, OrganizationRole.Administrator, null, null), cancellationToken);
        (await MembershipsAsync(colleague, cancellationToken))
            .Should().ContainSingle(m => m.OrganizationId == Demo.OrganizationId && m.Role == OrganizationRole.Administrator);

        await owner.PatchAsync($"{OrganizationApi}/members/{member.OrganizationMemberId}",
            new ChangeOrganizationMemberRoleRequest(OrganizationRole.LocationManager, Demo.CentralId, null), cancellationToken);
        (await owner.GetAsync<List<OrganizationMemberResponse>>($"{OrganizationApi}/members", cancellationToken))
            .Should().ContainSingle(m => m.OrganizationMemberId == member.OrganizationMemberId
                && m.Role == OrganizationRole.LocationManager && m.LocationId == Demo.CentralId);

        await owner.DeleteAsync($"{OrganizationApi}/members/{member.OrganizationMemberId}", cancellationToken);
        (await MembershipsAsync(colleague, cancellationToken)).Should().BeEmpty();
    }

    [Fact]
    public async Task LocationManager_SeesTheirLocation_AndIsRefusedAnother()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.Factory.CreateClient();
        var manager = await SignInAsync(client, "manager@demo.test", cancellationToken);
        var date = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        using var own = await manager.SendAsync(HttpMethod.Get,
            $"{OrganizationApi}/locations/{Demo.CentralId}/bookings?date={date}", null, cancellationToken);
        using var other = await manager.SendAsync(HttpMethod.Get,
            $"{OrganizationApi}/locations/{Demo.RiversideId}/bookings?date={date}", null, cancellationToken);

        own.StatusCode.Should().Be(HttpStatusCode.OK);
        other.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task HiddenReview_IsNotShownToGuests_UntilItIsShownAgain()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.Factory.CreateClient();
        var owner = await SignInAsync(client, DemoData.OwnerEmail, cancellationToken);
        var guest = new TestApi(client);
        var reviewsPath = $"{OrganizationApi}/locations/{Demo.CentralId}/reviews";
        var publicReviewsPath = $"{PublicApi}/employees/{Demo.AnnaId}/reviews";

        var review = (await owner.GetAsync<List<ReviewModerationResponse>>(reviewsPath, cancellationToken))
            .First(r => r.EmployeeId == Demo.AnnaId && !r.IsHidden);

        await owner.PostAsync($"{reviewsPath}/{review.ReviewId}/hide", null, cancellationToken);
        (await guest.GetAsync<List<ReviewResponse>>(publicReviewsPath, cancellationToken)).Should().NotContain(r => r.ReviewId == review.ReviewId);

        await owner.PostAsync($"{reviewsPath}/{review.ReviewId}/unhide", null, cancellationToken);
        (await guest.GetAsync<List<ReviewResponse>>(publicReviewsPath, cancellationToken)).Should().Contain(r => r.ReviewId == review.ReviewId);
    }

    [Fact]
    public async Task PastVisit_MarkedNoShow_GuestSeesItSo()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.Factory.CreateClient();
        var owner = await SignInAsync(client, DemoData.OwnerEmail, cancellationToken);
        var guest = new TestApi(client);

        await owner.PostAsync(
            $"{OrganizationApi}/locations/{Demo.CentralId}/employees/{Demo.AnnaId}/bookings/{Demo.OlegReview.Id}/no-show",
            null, cancellationToken);

        var visit = await guest.GetAsync<PublicBookingDetailsResponse>(
            $"{PublicApi}/bookings/{Demo.OlegReview.Id}?token={Uri.EscapeDataString(Demo.OlegReview.Token)}", cancellationToken);
        visit.Status.Should().Be(BookingStatus.NoShow);
    }

    [Fact]
    public async Task Organization_DeletedByItsOwner_IsGoneForThem()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.Factory.CreateClient();
        var founderToken = await TestHelpers.RegisterAndLoginAsync(client, "founder@demo.test");
        var organizationId = await TestHelpers.CreateOrganizationAsync(client, founderToken, "Short-lived", "short-lived");
        var founder = new TestApi(client, founderToken);

        await founder.DeleteAsync($"api/v1/organizations/{organizationId}", cancellationToken);

        (await MembershipsAsync(founder, cancellationToken)).Should().BeEmpty();
    }

    private static async Task<TestApi> SignInAsync(HttpClient client, string email, CancellationToken cancellationToken) =>
        new(client, await TestHelpers.SignInAsync(client, email, DevelopmentSeeder.Password, cancellationToken));

    private static Task<List<MyOrganizationMembershipResponse>> MembershipsAsync(TestApi account, CancellationToken cancellationToken) =>
        account.GetAsync<List<MyOrganizationMembershipResponse>>("api/v1/me/organizations", cancellationToken);

    private static async Task<PublicServiceResponse> PublicServiceAsync(
        TestApi guest, Guid locationId, Guid serviceId, CancellationToken cancellationToken) =>
        (await guest.GetAsync<List<PublicServiceResponse>>($"{PublicApi}/locations/{locationId}/services", cancellationToken))
            .Single(s => s.ServiceId == serviceId);

    private static Task<List<PublicEmployeeResponse>> PublicEmployeesAsync(TestApi guest, Guid locationId, CancellationToken cancellationToken) =>
        guest.GetAsync<List<PublicEmployeeResponse>>($"{PublicApi}/locations/{locationId}/employees", cancellationToken);

    private Task<List<AvailableSlotResponse>> AvailableHaircutsAsync(
        TestApi guest, Guid employeeId, DateOnly day, CancellationToken cancellationToken) =>
        guest.GetAsync<List<AvailableSlotResponse>>(
            $"{PublicApi}/locations/{Demo.RiversideId}/employees/{employeeId}/services/{Demo.HaircutId}/availability"
                + $"?date={day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}",
            cancellationToken);

    /// <summary>The 45-minute haircuts at Riverside fill exactly the hours from <paramref name="from"/> to <paramref name="to"/>, Moscow time.</summary>
    private async Task ShouldOfferHaircutsAsync(
        TestApi guest, Guid employeeId, DateOnly day, TimeOnly from, TimeOnly to, CancellationToken cancellationToken)
    {
        var slots = await AvailableHaircutsAsync(guest, employeeId, day, cancellationToken);
        var zone = TimeZoneInfo.FindSystemTimeZoneById(RiversideZone);

        slots.Should().NotBeEmpty();
        slots[0].StartUtc.Should().Be(TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(from), zone));
        slots[^1].EndUtc.Should().Be(TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(to), zone));
    }

    private static List<DailyHoursDto> WeekdaysNineToSix(DailyHoursDto? saturday) =>
    [
        .. Enum.GetValues<DayOfWeek>().Select(day => day switch
        {
            DayOfWeek.Saturday when saturday is not null => saturday,
            DayOfWeek.Saturday or DayOfWeek.Sunday => new DailyHoursDto(day, null, null),
            _ => new DailyHoursDto(day, new TimeOnly(9, 0), new TimeOnly(18, 0))
        })
    ];
}
