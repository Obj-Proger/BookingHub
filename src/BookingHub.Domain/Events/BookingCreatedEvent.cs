using BookingHub.Domain.ValueObjects;

namespace BookingHub.Domain.Events;

public sealed record BookingCreatedEvent(
    Guid BookingId, Guid OrganizationId, ClientContact ClientContact, SecurityToken ConfirmationToken, DateTime OccurredOnUtc) : IDomainEvent;