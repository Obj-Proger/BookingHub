namespace BookingHub.Domain.Common;

/// <summary>
/// A fact about an aggregate. Events are stored together with the change that raised them and
/// handled only after that change has been committed — a subscriber never reacts to state that
/// could still be rolled back.
/// </summary>
public interface IDomainEvent
{
    /// <summary>The organization the event belongs to; it is handled inside that organization's tenant scope.</summary>
    Guid OrganizationId { get; }

    /// <summary>Gets the UTC timestamp when the event occurred.</summary>
    DateTime OccurredOnUtc { get; }
}