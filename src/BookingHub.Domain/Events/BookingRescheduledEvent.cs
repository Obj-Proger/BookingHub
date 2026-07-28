using BookingHub.Domain.ValueObjects;

namespace BookingHub.Domain.Events;

public sealed record BookingRescheduledEvent(
    Guid BookingId, Guid OrganizationId, Guid LocationId, Guid EmployeeId, Guid ServiceId,
    TimeSlot NewTimeSlot, DateTime OccurredOnUtc) : IDomainEvent;