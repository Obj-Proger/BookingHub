namespace BookingHub.Domain.Common;

/// <summary>
/// Defines audit timestamp properties for entities that need a creation/modification trail.
/// Implementing entities should declare both properties with a private setter —
/// the values are populated by infrastructure, not by domain logic.
/// </summary>
public interface IAuditable
{
    /// <summary>Gets the UTC timestamp when the entity was created.</summary>
    DateTime CreatedAtUtc { get; }

    /// <summary>Gets the UTC timestamp when the entity was last modified.</summary>
    DateTime? ModifiedAtUtc { get; }
}