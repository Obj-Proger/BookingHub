using BookingHub.Mobile.Api;
using BookingHub.Mobile.Services;
using BookingHub.Mobile.ViewModels;
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
        builder.Services.AddSingleton<IAppSessionContext, AppSessionContext>();

        builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
            client.BaseAddress = new Uri(ApiConstants.BaseUrl));

        builder.Services.AddTransient<AuthTokenHandler>();

        builder.Services.AddHttpClient<IMeApiClient, MeApiClient>(client => client.BaseAddress = new Uri(ApiConstants.BaseUrl))
            .AddHttpMessageHandler<AuthTokenHandler>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<Views.LoginPage>();
        builder.Services.AddTransient<Views.SchedulePage>();

        return builder.Build();
    }
}