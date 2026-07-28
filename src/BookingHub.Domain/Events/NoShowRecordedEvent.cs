using BookingHub.Domain.ValueObjects;

namespace BookingHub.Domain.Events;

public sealed record NoShowRecordedEvent(
    Guid BookingId, Guid OrganizationId, Guid EmployeeId, ClientContact ClientContact, DateTime OccurredOnUtc) : IDomainEvent;