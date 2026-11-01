namespace BookingHub.Infrastructure.Notifications;

/// <param name="OrganizationId">The organization the message is sent on behalf of.</param>
public sealed record SmsMessage(Guid OrganizationId, string ToPhoneNumber, string Body);

/// <summary>Delivers one SMS. Called only by the outbox processor — see <see cref="IEmailService"/>.</summary>
public interface ISmsService
{
    Task SendAsync(SmsMessage message, CancellationToken cancellationToken);
}