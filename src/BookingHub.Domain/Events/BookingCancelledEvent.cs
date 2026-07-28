using BookingHub.Domain.ValueObjects;

namespace BookingHub.Domain.Events;

public sealed record BookingCancelledEvent(
    Guid BookingId, Guid OrganizationId, Guid LocationId, Guid EmployeeId, Guid ServiceId,
    TimeSlot TimeSlot, DateTime OccurredOnUtc) : IDomainEvent;