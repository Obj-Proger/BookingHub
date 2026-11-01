using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Common.Notifications;

/// <summary>
/// Queues a message to a guest: by email when the address is known, and always by SMS. Nothing is
/// sent here — the message is stored with the current unit of work and delivered only after that
/// save commits, so a guest is never told about a change that was rolled back.
/// </summary>
public interface IGuestNotifier
{
    void Notify(Guid organizationId, ClientContact recipient, string subject, string body);
}