using BookingHub.Domain.ValueObjects;

namespace BookingHub.Domain.Events;

public sealed record WaitlistSlotOfferedEvent(
    Guid WaitlistEntryId, Guid OrganizationId, ClientContact ClientContact,
    TimeSlot OfferedSlot, DateTime OfferExpiresAtUtc, DateTime OccurredOnUtc) : IDomainEvent;