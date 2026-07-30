namespace BookingHub.Domain.Tests.ValueObjects;

public class TimeSlotTests
{
    private static readonly DateTime Start = new(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_ValidUtcRange_Succeeds()
    {
        var result = TimeSlot.Create(Start, End);

        result.IsSuccess.Should().BeTrue();
        result.Value.Duration.Should().Be(TimeSpan.FromHours(1));
    }

    public static TheoryData<DateTime, DateTime> NonUtcCombinations() => new()
    {
        { DateTime.SpecifyKind(Start, DateTimeKind.Local), End },
        { Start, DateTime.SpecifyKind(End, DateTimeKind.Unspecified) },
    };

    [Theory]
    [MemberData(nameof(NonUtcCombinations))]
    public void Create_NonUtcTimestamps_FailsWithNotUtcError(DateTime start, DateTime end)
    {
        var result = TimeSlot.Create(start, end);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.TimeSlot.NotUtc);
    }

    [Fact]
    public void Create_StartNotBeforeEnd_FailsWithStartNotBeforeEndError()
    {
        var result = TimeSlot.Create(End, Start);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.TimeSlot.StartNotBeforeEnd);
    }

    [Fact]
    public void Overlaps_IntersectingSlots_ReturnsTrue()
    {
        var first = TimeSlot.Create(Start, End).Value;
        var second = TimeSlot.Create(Start.AddMinutes(30), End.AddMinutes(30)).Value;

        first.Overlaps(second).Should().BeTrue();
    }

    [Fact]
    public void Overlaps_AdjacentSlotsSharingOnlyABoundary_ReturnsFalse()
    {
        var first = TimeSlot.Create(Start, End).Value;
        var second = TimeSlot.Create(End, End.AddHours(1)).Value;

        first.Overlaps(second).Should().BeFalse();
    }

    [Fact]
    public void Contains_InstantAtStart_ReturnsTrue()
    {
        var slot = TimeSlot.Create(Start, End).Value;

        slot.Contains(Start).Should().BeTrue();
    }

    [Fact]
    public void Contains_InstantAtEnd_ReturnsFalse()
    {
        var slot = TimeSlot.Create(Start, End).Value;

        slot.Contains(End).Should().BeFalse();
    }
}