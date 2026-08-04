namespace BookingHub.Application.Common.Notifications;

/// <param name="OrganizationId">
/// Lets the Infrastructure implementation redirect demo-organization messages to a mock
/// outbox instead of sending them for real — see the Demo architecture discussion.
/// </param>
public sealed record EmailMessage(Guid OrganizationId, string ToAddress, string Subject, string Body);

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}