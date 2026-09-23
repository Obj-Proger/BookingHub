using System.Collections.ObjectModel;
using BookingHub.Mobile.Api;
using BookingHub.Mobile.Api.Contracts;
using BookingHub.Mobile.Domain;
using BookingHub.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookingHub.Mobile.ViewModels;

public enum LoginStep
{
    Credentials,
    SelectOrganization,
    SelectLocation
}

public sealed partial class LoginViewModel(
    IAuthApiClient authApiClient, IMeApiClient meApiClient, ISecureTokenStore tokenStore, IAppSessionContext sessionContext)
    : BaseViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCredentialsStepVisible))]
    [NotifyPropertyChangedFor(nameof(IsOrganizationStepVisible))]
    [NotifyPropertyChangedFor(nameof(IsLocationStepVisible))]
    public partial LoginStep Step { get; set; } = LoginStep.Credentials;

    public bool IsCredentialsStepVisible => Step == LoginStep.Credentials;
    public bool IsOrganizationStepVisible => Step == LoginStep.SelectOrganization;
    public bool IsLocationStepVisible => Step == LoginStep.SelectLocation;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    public ObservableCollection<MyOrganizationMembershipResponse> Organizations { get; } = [];
    public ObservableCollection<EmployeeLocationResponse> Locations { get; } = [];

    private MyOrganizationMembershipResponse? _pendingOrganization;

    [RelayCommand]
    private async Task TryResumeSessionAsync()
    {
        if (!sessionContext.IsComplete || await tokenStore.GetAccessTokenAsync() is null)
            return;

        IsBusy = true;
        try
        {
            var organizations = await meApiClient.GetMyOrganizationsAsync(CancellationToken.None);
            if (organizations is not null)
                await Shell.Current.GoToAsync("//schedule");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var authResult = await authApiClient.LoginAsync(Email, Password, CancellationToken.None);
            if (authResult is null)
            {
                ErrorMessage = "Неверный email или пароль.";
                return;
            }

            await tokenStore.SaveTokensAsync(authResult.AccessToken, authResult.RefreshToken);

            var organizations = await meApiClient.GetMyOrganizationsAsync(CancellationToken.None);
            var employeeOrganizations = organizations?.Where(o => o.Role == OrganizationRole.Employee && o.EmployeeId is not null).ToList() ?? [];

            if (employeeOrganizations.Count == 0)
            {
                ErrorMessage = "У вас нет ни одной организации, где вы числитесь сотрудником.";
                return;
            }

            if (employeeOrganizations.Count == 1)
            {
                await ProceedToLocationSelectionAsync(employeeOrganizations[0]);
                return;
            }

            Organizations.Clear();
            foreach (var organization in employeeOrganizations)
                Organizations.Add(organization);
            Step = LoginStep.SelectOrganization;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectOrganizationAsync(MyOrganizationMembershipResponse organization) =>
        await ProceedToLocationSelectionAsync(organization);

    private async Task ProceedToLocationSelectionAsync(MyOrganizationMembershipResponse organization)
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var locations = await meApiClient.GetMyLocationsAsync(organization.OrganizationId, CancellationToken.None);
            if (locations is null || locations.Count == 0)
            {
                ErrorMessage = "Вы не назначены ни на одну локацию в этой организации.";
                return;
            }

            if (locations.Count == 1)
            {
                CompleteLogin(organization, locations[0]);
                await Shell.Current.GoToAsync("//schedule");
                return;
            }

            _pendingOrganization = organization;
            Locations.Clear();
            foreach (var location in locations)
                Locations.Add(location);
            Step = LoginStep.SelectLocation;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectLocationAsync(EmployeeLocationResponse location)
    {
        if (_pendingOrganization is null)
            return;

        CompleteLogin(_pendingOrganization, location);
        await Shell.Current.GoToAsync("//schedule");
    }

    private void CompleteLogin(MyOrganizationMembershipResponse organization, EmployeeLocationResponse location) =>
        sessionContext.Save(organization.OrganizationId, organization.OrganizationName, location.LocationId, location.LocationName, organization.EmployeeId!.Value);
}