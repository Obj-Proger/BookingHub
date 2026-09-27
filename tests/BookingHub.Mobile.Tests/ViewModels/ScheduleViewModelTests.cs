namespace BookingHub.Mobile.Tests.ViewModels;

public class ScheduleViewModelTests
{
    private readonly IBookingsApiClient _bookingsApiClient = Substitute.For<IBookingsApiClient>();
    private readonly IAuthApiClient _authApiClient = Substitute.For<IAuthApiClient>();
    private readonly ISecureTokenStore _tokenStore = Substitute.For<ISecureTokenStore>();
    private readonly IAppSessionContext _sessionContext = Substitute.For<IAppSessionContext>();
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
    private readonly ILocalizationService _localization = Substitute.For<ILocalizationService>();

    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid LocationId = Guid.CreateVersion7();
    private static readonly Guid EmployeeId = Guid.CreateVersion7();

    public ScheduleViewModelTests()
    {
        _sessionContext.IsComplete.Returns(true);
        _sessionContext.OrganizationId.Returns(OrganizationId);
        _sessionContext.LocationId.Returns(LocationId);
        _sessionContext.EmployeeId.Returns(EmployeeId);
    }

    private ScheduleViewModel CreateSut() =>
        new(_bookingsApiClient, _authApiClient, _tokenStore, _sessionContext, _navigationService, _localization);

    private static EmployeeBookingResponse CreateBooking(DateTime startUtc, BookingStatus status = BookingStatus.Confirmed) =>
        new(Guid.CreateVersion7(), startUtc, startUtc.AddHours(1), "Haircut", "Jane Doe", "+14155552671", status);

    [Fact]
    public async Task LoadScheduleAsync_ApiReturnsNull_SetsErrorMessage()
    {
        _bookingsApiClient.GetScheduleAsync(OrganizationId, LocationId, EmployeeId, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<EmployeeBookingResponse>?)null);
        _localization["Schedule_Error_LoadFailed"].Returns("Couldn't load the schedule.");
        var sut = CreateSut();

        await sut.LoadScheduleCommand.ExecuteAsync(null);

        sut.ErrorMessage.Should().Be("Couldn't load the schedule.");
        sut.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task LoadScheduleAsync_ApiReturnsBookings_PopulatesSortedByStartTime()
    {
        var later = CreateBooking(DateTime.UtcNow.AddHours(3));
        var earlier = CreateBooking(DateTime.UtcNow.AddHours(1));
        _bookingsApiClient.GetScheduleAsync(OrganizationId, LocationId, EmployeeId, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([later, earlier]);
        var sut = CreateSut();

        await sut.LoadScheduleCommand.ExecuteAsync(null);

        sut.Bookings.Should().HaveCount(2);
        sut.Bookings[0].BookingId.Should().Be(earlier.BookingId);
        sut.Bookings[1].BookingId.Should().Be(later.BookingId);
    }

    [Fact]
    public async Task GoToNextDayAsync_AdvancesDateAndReloads()
    {
        var initialDate = DateOnly.FromDateTime(DateTime.Now);
        _bookingsApiClient.GetScheduleAsync(OrganizationId, LocationId, EmployeeId, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([]);
        var sut = CreateSut();

        await sut.GoToNextDayCommand.ExecuteAsync(null);

        sut.SelectedDate.Should().Be(initialDate.AddDays(1));
        await _bookingsApiClient.Received(1).GetScheduleAsync(OrganizationId, LocationId, EmployeeId, initialDate.AddDays(1), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OpenBookingAsync_NavigatesWithRawBooking()
    {
        var booking = CreateBooking(DateTime.UtcNow);
        _bookingsApiClient.GetScheduleAsync(OrganizationId, LocationId, EmployeeId, Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns([booking]);
        var sut = CreateSut();
        await sut.LoadScheduleCommand.ExecuteAsync(null);

        await sut.OpenBookingCommand.ExecuteAsync(sut.Bookings[0]);

        await _navigationService.Received(1).GoToAsync("bookingDetail",
            Arg.Is<IDictionary<string, object>>(d => (EmployeeBookingResponse)d["Booking"] == booking));
    }

    [Fact]
    public async Task LogoutAsync_ServerCallThrows_StillClearsSessionAndNavigatesToLogin()
    {
        _tokenStore.GetRefreshTokenAsync().Returns("refresh-token");
        _authApiClient.LogoutAsync("refresh-token", Arg.Any<CancellationToken>()).ThrowsAsync<HttpRequestException>();
        var sut = CreateSut();

        await sut.LogoutCommand.ExecuteAsync(null);

        await _tokenStore.Received(1).ClearAsync();
        _sessionContext.Received(1).Clear();
        await _navigationService.Received(1).GoToAsync("//login");
    }

    [Fact]
    public void ToggleLanguage_SwitchesCulture()
    {
        _localization.CurrentCulture = new CultureInfo("ru");
        var sut = CreateSut();

        sut.ToggleLanguageCommand.Execute(null);

        _localization.Received(1).CurrentCulture = Arg.Is<CultureInfo>(c => c.TwoLetterISOLanguageName == "en");
    }
}