namespace BookingHub.Application.Common.Notifications;

public sealed record SmsMessage(Guid OrganizationId, string ToPhoneNumber, string Body);

public interface ISmsService
{
    Task SendAsync(SmsMessage message, CancellationToken cancellationToken);
}