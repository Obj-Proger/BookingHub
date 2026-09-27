namespace BookingHub.Mobile.Tests.ViewModels;

public class BookingDetailViewModelTests
{
    private readonly IBookingsApiClient _bookingsApiClient = Substitute.For<IBookingsApiClient>();
    private readonly IAppSessionContext _sessionContext = Substitute.For<IAppSessionContext>();
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
    private readonly ILocalizationService _localization = Substitute.For<ILocalizationService>();

    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid LocationId = Guid.CreateVersion7();
    private static readonly Guid EmployeeId = Guid.CreateVersion7();
    private static readonly Guid BookingId = Guid.CreateVersion7();

    public BookingDetailViewModelTests()
    {
        _sessionContext.IsComplete.Returns(true);
        _sessionContext.OrganizationId.Returns(OrganizationId);
        _sessionContext.LocationId.Returns(LocationId);
        _sessionContext.EmployeeId.Returns(EmployeeId);
    }

    private BookingDetailViewModel CreateSut() => new(_bookingsApiClient, _sessionContext, _navigationService, _localization);

    private static EmployeeBookingResponse CreateBooking(BookingStatus status) =>
        new(BookingId, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), "Haircut", "Jane Doe", "+14155552671", status);

    [Fact]
    public void Initialize_StatusAwaitingReview_SetsCanResolveTrue()
    {
        var sut = CreateSut();

        sut.Initialize(CreateBooking(BookingStatus.AwaitingReview));

        sut.CanResolve.Should().BeTrue();
    }

    [Fact]
    public void Initialize_StatusConfirmed_SetsCanResolveFalse()
    {
        var sut = CreateSut();

        sut.Initialize(CreateBooking(BookingStatus.Confirmed));

        sut.CanResolve.Should().BeFalse();
    }

    [Fact]
    public async Task CompleteAsync_ApiSucceeds_NavigatesBack()
    {
        _bookingsApiClient.CompleteAsync(OrganizationId, LocationId, EmployeeId, BookingId, Arg.Any<CancellationToken>()).Returns(true);
        var sut = CreateSut();
        sut.Initialize(CreateBooking(BookingStatus.AwaitingReview));

        await sut.CompleteCommand.ExecuteAsync(null);

        await _navigationService.Received(1).GoToAsync("..");
        sut.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task CompleteAsync_ApiFails_SetsErrorMessageAndDoesNotNavigate()
    {
        _bookingsApiClient.CompleteAsync(OrganizationId, LocationId, EmployeeId, BookingId, Arg.Any<CancellationToken>()).Returns(false);
        _localization["BookingDetail_Error_ActionFailed"].Returns("Couldn't update the booking.");
        var sut = CreateSut();
        sut.Initialize(CreateBooking(BookingStatus.AwaitingReview));

        await sut.CompleteCommand.ExecuteAsync(null);

        sut.ErrorMessage.Should().Be("Couldn't update the booking.");
        await _navigationService.DidNotReceive().GoToAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task MarkNoShowAsync_ApiSucceeds_NavigatesBack()
    {
        _bookingsApiClient.MarkNoShowAsync(OrganizationId, LocationId, EmployeeId, BookingId, Arg.Any<CancellationToken>()).Returns(true);
        var sut = CreateSut();
        sut.Initialize(CreateBooking(BookingStatus.AwaitingReview));

        await sut.MarkNoShowCommand.ExecuteAsync(null);

        await _navigationService.Received(1).GoToAsync("..");
    }
}