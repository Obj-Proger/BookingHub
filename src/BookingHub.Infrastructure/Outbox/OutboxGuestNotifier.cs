using BookingHub.Application.Common.Notifications;
using BookingHub.Domain.ValueObjects;
using BookingHub.Infrastructure.Notifications;
using BookingHub.Infrastructure.Persistence;

namespace BookingHub.Infrastructure.Outbox;

/// <summary>
/// Adds the messages to the outbox in the caller's unit of work; <see cref="OutboxProcessor"/>
/// delivers them once it is saved. The email and the SMS are separate rows, so one channel failing
/// is retried without repeating the other.
/// </summary>
internal sealed class OutboxGuestNotifier(ApplicationDbContext dbContext, TimeProvider timeProvider) : IGuestNotifier
{
    public void Notify(Guid organizationId, ClientContact recipient, string subject, string body)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        if (recipient.Email is not null)
        {
            dbContext.OutboxMessages.Add(OutboxMessage.Create(
                organizationId, new EmailMessage(organizationId, recipient.Email.Value, subject, body), utcNow));
        }

        dbContext.OutboxMessages.Add(OutboxMessage.Create(
            organizationId, new SmsMessage(organizationId, recipient.Phone.Value, body), utcNow));
    }
}