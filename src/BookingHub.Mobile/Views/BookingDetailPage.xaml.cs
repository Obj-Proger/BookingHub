using BookingHub.Mobile.ViewModels;

namespace BookingHub.Mobile.Views;

public partial class BookingDetailPage : ContentPage
{
    public BookingDetailPage(BookingDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}