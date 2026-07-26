namespace BookingHub.Domain.Common;

/// <summary>Defines the contract for aggregates that raise domain events.</summary>
public interface IHasDomainEvents
{
    /// <summary>Gets the collection of domain events raised by this aggregate.</summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>Clears all domain events after they have been dispatched.</summary>
    void ClearDomainEvents();
}