using BookingHub.Mobile.ViewModels;

namespace BookingHub.Mobile.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LoginViewModel viewModel)
            viewModel.TryResumeSessionCommand.Execute(null);
    }
}