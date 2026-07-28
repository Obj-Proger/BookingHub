using BookingHub.Domain.ValueObjects;

namespace BookingHub.Domain.Events;

public sealed record BookingConfirmedEvent(
    Guid BookingId, Guid OrganizationId, Guid LocationId, Guid EmployeeId,
    ClientContact ClientContact, TimeSlot TimeSlot, DateTime OccurredOnUtc) : IDomainEvent;