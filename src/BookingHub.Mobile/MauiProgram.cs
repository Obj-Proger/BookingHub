using BookingHub.Mobile.Api;
using BookingHub.Mobile.Services;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace BookingHub.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<ISecureTokenStore, SecureTokenStore>();

        // No AuthTokenHandler here — every /auth/* endpoint is anonymous, and Refresh must
        // never route through the handler that calls it (see AuthTokenHandler's own remarks).
        builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
            client.BaseAddress = new Uri(ApiConstants.BaseUrl));

        builder.Services.AddTransient<AuthTokenHandler>();

        // Every other typed API client (Organizations, Bookings — added in later commits) is
        // built on this named client instead, carrying the bearer token automatically.
        builder.Services.AddHttpClient("Api", client => client.BaseAddress = new Uri(ApiConstants.BaseUrl))
            .AddHttpMessageHandler<AuthTokenHandler>();

        return builder.Build();
    }
}