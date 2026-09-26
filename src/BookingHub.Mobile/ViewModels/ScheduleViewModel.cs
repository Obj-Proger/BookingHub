using BookingHub.Mobile.Api;
using BookingHub.Mobile.Api.Contracts;
using BookingHub.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalizationResourceManager.Maui;
using System.Collections.ObjectModel;

namespace BookingHub.Mobile.ViewModels;

public sealed record ScheduleBookingItem(
    Guid BookingId, string TimeRange, string ServiceName, string ClientDisplay, string StatusDisplay, EmployeeBookingResponse Raw);

public sealed partial class ScheduleViewModel(
    IBookingsApiClient bookingsApiClient, IAppSessionContext sessionContext, ILocalizationResourceManager localization)
    : BaseViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedDateDisplay))]
    public partial DateOnly SelectedDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public string SelectedDateDisplay => SelectedDate.ToString("d", localization.CurrentCulture);

    public ObservableCollection<ScheduleBookingItem> Bookings { get; } = [];

    [RelayCommand]
    private async Task LoadScheduleAsync()
    {
        if (!sessionContext.IsComplete)
            return;

        ErrorMessage = null;
        IsBusy = true;
        try
        {
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
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task GoToPreviousDayAsync() => ChangeDateAsync(SelectedDate.AddDays(-1));

    [RelayCommand]
    private Task GoToNextDayAsync() => ChangeDateAsync(SelectedDate.AddDays(1));

    [RelayCommand]
    private Task GoToTodayAsync() => ChangeDateAsync(DateOnly.FromDateTime(DateTime.Now));

    [RelayCommand]
    private static async Task OpenBookingAsync(ScheduleBookingItem item) =>
        await Shell.Current.GoToAsync("bookingDetail", new Dictionary<string, object> { ["Booking"] = item.Raw });
    
    private Task ChangeDateAsync(DateOnly date)
    {
        SelectedDate = date;
        return LoadScheduleAsync();
    }

}