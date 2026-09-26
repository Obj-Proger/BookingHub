using BookingHub.Mobile.ViewModels;

namespace BookingHub.Mobile.Views;

public partial class SchedulePage : ContentPage
{
    public SchedulePage(ScheduleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ScheduleViewModel viewModel)
            viewModel.LoadScheduleCommand.Execute(null);
    }
}