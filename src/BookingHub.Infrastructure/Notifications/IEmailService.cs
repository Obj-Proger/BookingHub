namespace BookingHub.Infrastructure.Notifications;

/// <param name="OrganizationId">The organization the message is sent on behalf of.</param>
public sealed record EmailMessage(Guid OrganizationId, string ToAddress, string Subject, string Body);

/// <summary>
/// Delivers one email. Called only by the outbox processor — Application queues messages through
/// IGuestNotifier, so nothing is sent for a change that was rolled back.
/// </summary>
public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}