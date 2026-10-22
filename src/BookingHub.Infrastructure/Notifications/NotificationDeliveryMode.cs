namespace BookingHub.Infrastructure.Notifications;

public enum NotificationDeliveryMode
{
    /// <summary>Messages are delivered through the configured SMTP server and Twilio account.</summary>
    Live,

    /// <summary>Messages are written to the application log and nothing leaves the process.</summary>
    LogOnly
}