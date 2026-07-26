namespace BookingHub.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// Provides a unique identifier and domain event support.
/// </summary>
public abstract class BaseEntity : IHasDomainEvents, IEquatable<BaseEntity>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <param name="id">The identifier, assigned by the aggregate's own factory method.</param>
    protected BaseEntity(Guid id)
    {
        Id = id;
    }

    /// <summary>Required by EF Core for entity materialization.</summary>
    protected BaseEntity()
    {
    }

    /// <summary>Gets the unique identifier of the entity.</summary>
    public Guid Id { get; protected init; }

    /// <inheritdoc />
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Registers a domain event to be dispatched after the unit of work completes.</summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <inheritdoc />
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <inheritdoc />
    public bool Equals(BaseEntity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as BaseEntity);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(BaseEntity? left, BaseEntity? right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(BaseEntity? left, BaseEntity? right) => !(left == right);
}