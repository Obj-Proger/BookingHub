namespace BookingHub.Mobile.Services;

internal sealed class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route) => Shell.Current.GoToAsync(route);

    public Task GoToAsync(string route, IDictionary<string, object> parameters) => Shell.Current.GoToAsync(route, parameters);
}