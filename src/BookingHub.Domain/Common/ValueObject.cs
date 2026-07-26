namespace BookingHub.Domain.Common;

/// <summary>
/// Base class for Value Objects.
/// A Value Object is an immutable object whose equality is determined
/// by its component values rather than by identity.
/// </summary>
/// <remarks>
/// Subclasses must implement <see cref="GetEqualityComponents"/>
/// to participate in value-based equality comparison.
/// </remarks>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>Returns the components used to determine equality between instances.</summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc />
    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ValueObject other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        GetEqualityComponents().Aggregate(0, (hash, component) => HashCode.Combine(hash, component?.GetHashCode() ?? 0));

    public static bool operator ==(ValueObject? left, ValueObject? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}