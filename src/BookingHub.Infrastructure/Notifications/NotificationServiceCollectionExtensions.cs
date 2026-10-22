using BookingHub.Application.Common.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Twilio.Clients;

namespace BookingHub.Infrastructure.Notifications;

internal static class NotificationServiceCollectionExtensions
{
    private const string DeliveryModeKey = "Notifications:DeliveryMode";

    public static IServiceCollection AddNotifications(this IServiceCollection services, IConfiguration configuration)
    {
        var deliveryMode = configuration.GetValue(DeliveryModeKey, NotificationDeliveryMode.Live);

        if (deliveryMode == NotificationDeliveryMode.LogOnly)
        {
            services.AddScoped<IEmailService, LoggingEmailService>();
            services.AddScoped<ISmsService, LoggingSmsService>();
            return services;
        }

        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.AddScoped<IEmailService, SmtpEmailService>();

        services.Configure<TwilioOptions>(configuration.GetSection(TwilioOptions.SectionName));

        var twilioOptions = configuration.GetSection(TwilioOptions.SectionName).Get<TwilioOptions>()
            ?? throw new InvalidOperationException("The \"Twilio\" configuration section is missing.");
        services.AddSingleton<ITwilioRestClient>(new TwilioRestClient(twilioOptions.AccountSid, twilioOptions.AuthToken));

        services.AddScoped<ISmsService, TwilioSmsService>();

        return services;
    }
}