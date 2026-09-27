using BookingHub.Mobile.Api.Contracts;
using BookingHub.Mobile.ViewModels;

namespace BookingHub.Mobile.Views;

public partial class BookingDetailPage : ContentPage, IQueryAttributable
{
    private readonly BookingDetailViewModel _viewModel;

    public BookingDetailPage(BookingDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query["Booking"] is EmployeeBookingResponse booking)
            _viewModel.Initialize(booking);
    }
}