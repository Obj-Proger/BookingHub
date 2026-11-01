using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName))
            .Validate(
                options => HasValues(options.Host, options.Username, options.Password, options.FromAddress),
                "Smtp:Host, Smtp:Username, Smtp:Password and Smtp:FromAddress are required when Notifications:DeliveryMode is Live.")
            .ValidateOnStart();
        services.AddScoped<IEmailService, SmtpEmailService>();

        services.AddOptions<TwilioOptions>()
            .Bind(configuration.GetSection(TwilioOptions.SectionName))
            .Validate(
                options => HasValues(options.AccountSid, options.AuthToken, options.FromPhoneNumber),
                "Twilio:AccountSid, Twilio:AuthToken and Twilio:FromPhoneNumber are required when Notifications:DeliveryMode is Live.")
            .ValidateOnStart();

        services.AddSingleton<ITwilioRestClient>(serviceProvider =>
        {
            var twilioOptions = serviceProvider.GetRequiredService<IOptions<TwilioOptions>>().Value;
            return new TwilioRestClient(twilioOptions.AccountSid, twilioOptions.AuthToken);
        });

        services.AddScoped<ISmsService, TwilioSmsService>();

        return services;
    }

    private static bool HasValues(params string?[] values) => values.All(value => !string.IsNullOrWhiteSpace(value));
}