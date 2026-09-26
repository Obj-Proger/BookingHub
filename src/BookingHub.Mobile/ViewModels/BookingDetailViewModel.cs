using BookingHub.Mobile.Api;
using BookingHub.Mobile.Api.Contracts;
using BookingHub.Mobile.Domain;
using BookingHub.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalizationResourceManager.Maui;

namespace BookingHub.Mobile.ViewModels;

public sealed partial class BookingDetailViewModel(
    IBookingsApiClient bookingsApiClient, IAppSessionContext sessionContext, ILocalizationResourceManager localization)
    : BaseViewModel, IQueryAttributable
{
    private Guid _bookingId;

    [ObservableProperty]
    public partial string TimeRange { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ServiceName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ClientName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ClientPhone { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool CanResolve { get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query["Booking"] is not EmployeeBookingResponse booking)
            return;

        _bookingId = booking.BookingId;
        TimeRange = $"{booking.StartUtc.ToLocalTime():HH:mm} – {booking.EndUtc.ToLocalTime():HH:mm}";
        ServiceName = booking.ServiceName;
        ClientName = string.IsNullOrWhiteSpace(booking.ClientName) ? localization["BookingDetail_NoClientName"] : booking.ClientName;
        ClientPhone = booking.ClientPhone;
        StatusDisplay = localization[$"BookingStatus_{booking.Status}"];
        CanResolve = booking.Status == BookingStatus.AwaitingReview;
    }

    [RelayCommand]
    private Task CompleteAsync() => ResolveAsync(bookingsApiClient.CompleteAsync);

    [RelayCommand]
    private Task MarkNoShowAsync() => ResolveAsync(bookingsApiClient.MarkNoShowAsync);

    private async Task ResolveAsync(Func<Guid, Guid, Guid, Guid, CancellationToken, Task<bool>> apiCall)
    {
        if (!sessionContext.IsComplete)
            return;

        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var succeeded = await apiCall(
                sessionContext.OrganizationId!.Value, sessionContext.LocationId!.Value, sessionContext.EmployeeId!.Value,
                _bookingId, CancellationToken.None);

            if (!succeeded)
            {
                ErrorMessage = localization["BookingDetail_Error_ActionFailed"];
                return;
            }

            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }
}