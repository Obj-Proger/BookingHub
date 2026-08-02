using BookingHub.Domain.Tests.TestDoubles;

namespace BookingHub.Domain.Tests.Common;

public class ValueObjectTests
{
    [Fact]
    public void Equals_SameComponentValues_ReturnsTrue()
    {
        var first = new SampleValueObject("a", 1);
        var second = new SampleValueObject("a", 1);

        first.Equals(second).Should().BeTrue();
        (first == second).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentComponentValues_ReturnsFalse()
    {
        var first = new SampleValueObject("a", 1);
        var second = new SampleValueObject("a", 2);

        first.Equals(second).Should().BeFalse();
        (first != second).Should().BeTrue();
    }

    [Fact]
    public void Equals_ComparedToNull_ReturnsFalse()
    {
        var value = new SampleValueObject("a", 1);

        (value == null).Should().BeFalse();
        (null == value).Should().BeFalse();
    }

    [Fact]
    public void Equals_BothNull_ReturnsTrue()
    {
        SampleValueObject? first = null;
        SampleValueObject? second = null;

        (first == second).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_EqualInstances_ReturnsSameHashCode()
    {
        var first = new SampleValueObject("a", 1);
        var second = new SampleValueObject("a", 1);

        first.GetHashCode().Should().Be(second.GetHashCode());
    }
}