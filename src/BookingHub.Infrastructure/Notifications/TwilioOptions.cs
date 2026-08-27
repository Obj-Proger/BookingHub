namespace BookingHub.Infrastructure.Notifications;

public sealed class TwilioOptions
{
    public const string SectionName = "Twilio";

    public required string AccountSid { get; init; }
    public required string AuthToken { get; init; }
    public required string FromPhoneNumber { get; init; }
}