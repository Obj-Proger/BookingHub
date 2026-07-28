using BookingHub.Domain.ValueObjects;

namespace BookingHub.Domain.Events;

public sealed record BookingCompletedEvent(
    Guid BookingId, Guid OrganizationId, Guid EmployeeId, ClientContact ClientContact, DateTime OccurredOnUtc) : IDomainEvent;