namespace BookingHub.Domain.Tests.TestDoubles;

internal sealed record SampleDomainEvent(DateTime OccurredOnUtc) : IDomainEvent;