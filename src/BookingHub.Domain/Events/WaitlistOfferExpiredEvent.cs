using BookingHub.Domain.ValueObjects;

namespace BookingHub.Domain.Events;

/// <summary>
/// Raised when an unconfirmed waitlist offer lapses, so a handler can offer
/// the same freed slot to the next queued entry.
/// </summary>
public sealed record WaitlistOfferExpiredEvent(
    Guid WaitlistEntryId, Guid OrganizationId, Guid LocationId, Guid ServiceId,
    Guid OfferedEmployeeId, TimeSlot OfferedSlot, DateTime OccurredOnUtc) : IDomainEvent;