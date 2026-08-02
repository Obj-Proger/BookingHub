namespace BookingHub.Domain.Tests.TestDoubles;

/// <summary>A second, distinct entity type — used to test that equality also checks concrete type, not just Id.</summary>
internal sealed class AnotherSampleEntity : BaseEntity
{
    public AnotherSampleEntity(Guid id) : base(id)
    {
    }
}