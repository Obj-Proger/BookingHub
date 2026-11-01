namespace BookingHub.Domain.Tests.TestDoubles;

internal sealed record SampleDomainEvent(Guid OrganizationId, DateTime OccurredOnUtc) : IDomainEvent;