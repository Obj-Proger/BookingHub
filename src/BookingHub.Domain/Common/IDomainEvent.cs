namespace BookingHub.Domain.Common;

/// <summary>
/// Marker interface for domain events, dispatched after the aggregate's
/// state has been persisted to the database.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Gets the UTC timestamp when the event occurred.</summary>
    DateTime OccurredOnUtc { get; }
}