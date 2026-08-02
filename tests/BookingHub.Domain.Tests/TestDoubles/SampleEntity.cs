namespace BookingHub.Domain.Tests.TestDoubles;

/// <summary>A minimal concrete <see cref="BaseEntity"/>, used only to exercise the base class's identity and domain-event logic.</summary>
internal sealed class SampleEntity : BaseEntity
{
    public SampleEntity(Guid id) : base(id)
    {
    }

    /// <summary>Exposes the protected <c>RaiseDomainEvent</c> for testing.</summary>
    public void RaiseSampleEvent(IDomainEvent domainEvent) => RaiseDomainEvent(domainEvent);
}