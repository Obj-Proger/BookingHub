namespace BookingHub.Mobile.Tests.ViewModels;

public class LoginViewModelTests
{
    private readonly IAuthApiClient _authApiClient = Substitute.For<IAuthApiClient>();
    private readonly IMeApiClient _meApiClient = Substitute.For<IMeApiClient>();
    private readonly ISecureTokenStore _tokenStore = Substitute.For<ISecureTokenStore>();
    private readonly IAppSessionContext _sessionContext = Substitute.For<IAppSessionContext>();
    private readonly INavigationService _navigationService = Substitute.For<INavigationService>();
    private readonly ILocalizationService _localization = Substitute.For<ILocalizationService>();

    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid LocationId = Guid.CreateVersion7();
    private static readonly Guid EmployeeId = Guid.CreateVersion7();

    private LoginViewModel CreateSut() =>
        new(_authApiClient, _meApiClient, _tokenStore, _sessionContext, _navigationService, _localization);

    [Fact]
    public async Task LoginAsync_InvalidCredentials_SetsErrorMessage()
    {
        _authApiClient.LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((AuthenticatedResponse?)null);
        _localization["Login_Error_InvalidCredentials"].Returns("Invalid email or password.");
        var sut = CreateSut();

        await sut.LoginCommand.ExecuteAsync(null);

        sut.ErrorMessage.Should().Be("Invalid email or password.");
        sut.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_NoEmployeeMembership_SetsErrorMessage()
    {
        _authApiClient.LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new AuthenticatedResponse("access", "refresh"));
        _meApiClient.GetMyOrganizationsAsync(Arg.Any<CancellationToken>())
            .Returns([new MyOrganizationMembershipResponse(OrganizationId, "Acme", OrganizationRole.Administrator, null, null)]);
        _localization["Login_Error_NoEmployeeMembership"].Returns("You have no employee membership.");
        var sut = CreateSut();

        await sut.LoginCommand.ExecuteAsync(null);

        sut.ErrorMessage.Should().Be("You have no employee membership.");
    }

    [Fact]
    public async Task LoginAsync_SingleEmployeeOrganizationWithSingleLocation_SavesSessionAndNavigatesToSchedule()
    {
        _authApiClient.LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new AuthenticatedResponse("access", "refresh"));
        _meApiClient.GetMyOrganizationsAsync(Arg.Any<CancellationToken>())
            .Returns([new MyOrganizationMembershipResponse(OrganizationId, "Acme", OrganizationRole.Employee, null, EmployeeId)]);
        _meApiClient.GetMyLocationsAsync(OrganizationId, Arg.Any<CancellationToken>())
            .Returns([new EmployeeLocationResponse(LocationId, "Downtown")]);
        var sut = CreateSut();

        await sut.LoginCommand.ExecuteAsync(null);

        _sessionContext.Received(1).Save(OrganizationId, "Acme", LocationId, "Downtown", EmployeeId);
        await _navigationService.Received(1).GoToAsync("//schedule");
    }

    [Fact]
    public async Task LoginAsync_MultipleEmployeeOrganizations_ShowsOrganizationStepWithoutNavigating()
    {
        _authApiClient.LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new AuthenticatedResponse("access", "refresh"));
        _meApiClient.GetMyOrganizationsAsync(Arg.Any<CancellationToken>()).Returns(
        [
            new MyOrganizationMembershipResponse(OrganizationId, "Acme", OrganizationRole.Employee, null, EmployeeId),
            new MyOrganizationMembershipResponse(Guid.CreateVersion7(), "Beta", OrganizationRole.Employee, null, Guid.CreateVersion7())
        ]);
        var sut = CreateSut();

        await sut.LoginCommand.ExecuteAsync(null);

        sut.Step.Should().Be(LoginStep.SelectOrganization);
        sut.Organizations.Should().HaveCount(2);
        await _navigationService.DidNotReceive().GoToAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task TryResumeSessionAsync_SessionIncomplete_DoesNothing()
    {
        _sessionContext.IsComplete.Returns(false);
        var sut = CreateSut();

        await sut.TryResumeSessionCommand.ExecuteAsync(null);

        await _meApiClient.DidNotReceive().GetMyOrganizationsAsync(Arg.Any<CancellationToken>());
        await _navigationService.DidNotReceive().GoToAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task TryResumeSessionAsync_TokenStillValid_NavigatesToSchedule()
    {
        _sessionContext.IsComplete.Returns(true);
        _tokenStore.GetAccessTokenAsync().Returns("valid-access-token");
        _meApiClient.GetMyOrganizationsAsync(Arg.Any<CancellationToken>()).Returns([]);
        var sut = CreateSut();

        await sut.TryResumeSessionCommand.ExecuteAsync(null);

        await _navigationService.Received(1).GoToAsync("//schedule");
    }

    [Fact]
    public void ToggleLanguage_SwitchesBetweenRuAndEn()
    {
        _localization.CurrentCulture = new CultureInfo("en");
        var sut = CreateSut();

        sut.ToggleLanguageCommand.Execute(null);

        _localization.Received(1).CurrentCulture = Arg.Is<CultureInfo>(c => c.TwoLetterISOLanguageName == "ru");
    }
}