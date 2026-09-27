using System.Collections.ObjectModel;
using System.Globalization;
using BookingHub.Mobile.Api;
using BookingHub.Mobile.Api.Contracts;
using BookingHub.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookingHub.Mobile.ViewModels;

public sealed record ScheduleBookingItem(
    Guid BookingId, string TimeRange, string ServiceName, string ClientDisplay, string StatusDisplay, EmployeeBookingResponse Raw);

public sealed partial class ScheduleViewModel(
    IBookingsApiClient bookingsApiClient, IAuthApiClient authApiClient, ISecureTokenStore tokenStore,
    IAppSessionContext sessionContext, INavigationService navigationService, ILocalizationService localization)
    : BaseViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedDateDisplay))]
    public partial DateOnly SelectedDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

    public string SelectedDateDisplay => SelectedDate.ToString("d", localization.CurrentCulture);

    public ObservableCollection<ScheduleBookingItem> Bookings { get; } = [];

    [RelayCommand]
    private async Task LoadScheduleAsync()
    {
        IsBusy = true;
        try
        {
            await LoadCoreAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            await LoadCoreAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task LoadCoreAsync()
    {
        if (!sessionContext.IsComplete)
            return;

        ErrorMessage = null;
        var bookings = await bookingsApiClient.GetScheduleAsync(
            sessionContext.OrganizationId!.Value, sessionContext.LocationId!.Value, sessionContext.EmployeeId!.Value,
            SelectedDate, CancellationToken.None);

        if (bookings is null)
        {
            ErrorMessage = localization["Schedule_Error_LoadFailed"];
            return;
        }

        Bookings.Clear();
        foreach (var booking in bookings.OrderBy(b => b.StartUtc))
        {
            Bookings.Add(new ScheduleBookingItem(
                booking.BookingId,
                $"{booking.StartUtc.ToLocalTime():HH:mm} – {booking.EndUtc.ToLocalTime():HH:mm}",
                booking.ServiceName,
                string.IsNullOrWhiteSpace(booking.ClientName) ? booking.ClientPhone : booking.ClientName,
                localization[$"BookingStatus_{booking.Status}"],
                booking));
        }
    }

    [RelayCommand]
    private Task GoToPreviousDayAsync() => ChangeDateAsync(SelectedDate.AddDays(-1));

    [RelayCommand]
    private Task GoToNextDayAsync() => ChangeDateAsync(SelectedDate.AddDays(1));

    [RelayCommand]
    private Task GoToTodayAsync() => ChangeDateAsync(DateOnly.FromDateTime(DateTime.Now));

    private Task ChangeDateAsync(DateOnly date)
    {
        SelectedDate = date;
        return LoadScheduleAsync();
    }

    [RelayCommand]
    private async Task OpenBookingAsync(ScheduleBookingItem item) =>
        await navigationService.GoToAsync("bookingDetail", new Dictionary<string, object> { ["Booking"] = item.Raw });

    [RelayCommand]
    private async Task LogoutAsync()
    {
        IsBusy = true;
        try
        {
            var refreshToken = await tokenStore.GetRefreshTokenAsync();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    await authApiClient.LogoutAsync(refreshToken, CancellationToken.None);
                }
                catch (HttpRequestException)
                {
                    // Best-effort: server-side token revocation is nice to have, but local
                    // logout must succeed even when there's no network at all.
                }
            }

            await tokenStore.ClearAsync();
            sessionContext.Clear();
            await navigationService.GoToAsync("//login");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ToggleLanguage()
    {
        var newCultureCode = localization.CurrentCulture.TwoLetterISOLanguageName == "ru" ? "en" : "ru";
        localization.CurrentCulture = new CultureInfo(newCultureCode);
    }
}