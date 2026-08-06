using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Notifications;
using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Events;

namespace BookingHub.Application.Features.Waitlist.EventHandlers;

internal sealed class WaitlistSlotOfferedNotificationHandler(
    IWaitlistEntryRepository waitlistEntryRepository, IEmailService emailService, ISmsService smsService)
    : IDomainEventHandler<WaitlistSlotOfferedEvent>
{
    public async Task Handle(WaitlistSlotOfferedEvent domainEvent, CancellationToken cancellationToken)
    {
        var entry = await waitlistEntryRepository.GetByIdAsync(domainEvent.WaitlistEntryId, cancellationToken);
        if (entry is null)
            return;

        var link = $"/waitlist/{entry.Id}/confirm?token={entry.ManagementToken.Value}";
        var body = $"A slot opened up: {domainEvent.OfferedSlot.StartUtc:u}. Confirm by {domainEvent.OfferExpiresAtUtc:u}: {link}";

        if (domainEvent.ClientContact.Email is not null)
            await emailService.SendAsync(new EmailMessage(domainEvent.OrganizationId, domainEvent.ClientContact.Email.Value, "A slot opened up", body), cancellationToken);

        await smsService.SendAsync(new SmsMessage(domainEvent.OrganizationId, domainEvent.ClientContact.Phone.Value, body), cancellationToken);
    }
}