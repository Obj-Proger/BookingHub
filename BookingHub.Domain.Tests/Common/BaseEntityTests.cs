using BookingHub.Domain.Tests.TestDoubles;

namespace BookingHub.Domain.Tests.Common;

public class BaseEntityTests
{
    [Fact]
    public void Equals_SameId_ReturnsTrue()
    {
        var id = Guid.CreateVersion7();
        var first = new SampleEntity(id);
        var second = new SampleEntity(id);

        first.Equals(second).Should().BeTrue();
        (first == second).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var first = new SampleEntity(Guid.CreateVersion7());
        var second = new SampleEntity(Guid.CreateVersion7());

        first.Equals(second).Should().BeFalse();
    }

    [Fact]
    public void Equals_SameIdButDifferentEntityType_ReturnsFalse()
    {
        var id = Guid.CreateVersion7();
        var sample = new SampleEntity(id);
        var other = new AnotherSampleEntity(id);

        sample.Equals(other).Should().BeFalse();
    }

    [Fact]
    public void RaiseDomainEvent_AddsEventToDomainEvents()
    {
        var entity = new SampleEntity(Guid.CreateVersion7());
        var domainEvent = new SampleDomainEvent(DateTime.UtcNow);

        entity.RaiseSampleEvent(domainEvent);

        entity.DomainEvents.Should().ContainSingle().Which.Should().Be(domainEvent);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllRaisedEvents()
    {
        var entity = new SampleEntity(Guid.CreateVersion7());
        entity.RaiseSampleEvent(new SampleDomainEvent(DateTime.UtcNow));

        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }
}