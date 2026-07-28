namespace BookingHub.Domain.Events;

public sealed record ReviewSubmittedEvent(
    Guid ReviewId, Guid OrganizationId, Guid EmployeeId, int Rating, DateTime OccurredOnUtc) : IDomainEvent;