using BookingHub.Domain.Services;

namespace BookingHub.Domain.Tests.Services;

public class AvailabilityCalculatorTests
{
    private static readonly Guid AssignmentId = Guid.CreateVersion7();
    private static readonly DateOnly Monday = new(2026, 3, 9);
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    private static DateTime ToUtc(int hour, int minute) => Monday.ToDateTime(new TimeOnly(hour, minute), DateTimeKind.Utc);

    private static WeeklyHours OpenAllDay(TimeOnly open, TimeOnly close) =>
        WeeklyHours.Create(Enum.GetValues<DayOfWeek>().Select(day => DailyHours.CreateOpen(day, open, close).Value)).Value;

    private static WeeklyHours ClosedAllDays() =>
        WeeklyHours.Create(Enum.GetValues<DayOfWeek>().Select(DailyHours.CreateClosed)).Value;

    private static RecurringSchedule Schedule(DayOfWeek day, TimeOnly start, TimeOnly end) =>
        RecurringSchedule.Create(AssignmentId, day, start, end).Value;

    [Fact]
    public void CalculateAvailableSlots_LocationClosedOnThatDay_ReturnsEmpty()
    {
        var slots = AvailabilityCalculator.CalculateAvailableSlots(
            ClosedAllDays(), [Schedule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0))], null,
            [], TimeSpan.FromMinutes(30), TimeSpan.Zero, TimeSpan.Zero, Monday, Utc, TimeSpan.FromMinutes(30));

        slots.Should().BeEmpty();
    }

    [Fact]
    public void CalculateAvailableSlots_NoRecurringScheduleForDay_ReturnsEmpty()
    {
        var slots = AvailabilityCalculator.CalculateAvailableSlots(
            OpenAllDay(new TimeOnly(9, 0), new TimeOnly(17, 0)), [], null,
            [], TimeSpan.FromMinutes(30), TimeSpan.Zero, TimeSpan.Zero, Monday, Utc, TimeSpan.FromMinutes(30));

        slots.Should().BeEmpty();
    }

    [Fact]
    public void CalculateAvailableSlots_DayOffException_ReturnsEmptyEvenWithRecurringSchedule()
    {
        var exception = ScheduleException.CreateDayOff(AssignmentId, Monday).Value;

        var slots = AvailabilityCalculator.CalculateAvailableSlots(
            OpenAllDay(new TimeOnly(9, 0), new TimeOnly(17, 0)),
            [Schedule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0))], exception,
            [], TimeSpan.FromMinutes(30), TimeSpan.Zero, TimeSpan.Zero, Monday, Utc, TimeSpan.FromMinutes(30));

        slots.Should().BeEmpty();
    }

    [Fact]
    public void CalculateAvailableSlots_ModifiedHoursException_UsesExceptionWindowNotRecurringSchedule()
    {
        var exception = ScheduleException.CreateModifiedHours(AssignmentId, Monday, new TimeOnly(12, 0), new TimeOnly(13, 0)).Value;

        var slots = AvailabilityCalculator.CalculateAvailableSlots(
            OpenAllDay(new TimeOnly(0, 0), new TimeOnly(23, 59)),
            [Schedule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0))], exception,
            [], TimeSpan.FromMinutes(30), TimeSpan.Zero, TimeSpan.Zero, Monday, Utc, TimeSpan.FromMinutes(30));

        slots.Should().HaveCount(2);
        slots.Select(s => s.StartUtc.TimeOfDay).Should().Equal(new TimeSpan(12, 0, 0), new TimeSpan(12, 30, 0));
    }

    [Fact]
    public void CalculateAvailableSlots_OpenWindowNoConflicts_GeneratesSlotsAtGranularity()
    {
        var slots = AvailabilityCalculator.CalculateAvailableSlots(
            OpenAllDay(new TimeOnly(9, 0), new TimeOnly(17, 0)),
            [Schedule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0))], null,
            [], TimeSpan.FromMinutes(30), TimeSpan.Zero, TimeSpan.Zero, Monday, Utc, TimeSpan.FromMinutes(30));

        slots.Should().HaveCount(16);
        slots[0].StartUtc.TimeOfDay.Should().Be(new TimeSpan(9, 0, 0));
        slots[^1].StartUtc.TimeOfDay.Should().Be(new TimeSpan(16, 30, 0));
    }

    [Fact]
    public void CalculateAvailableSlots_LocationHoursNarrowerThanEmployeeSchedule_ClipsToLocationHours()
    {
        var slots = AvailabilityCalculator.CalculateAvailableSlots(
            OpenAllDay(new TimeOnly(10, 0), new TimeOnly(14, 0)),
            [Schedule(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(18, 0))], null,
            [], TimeSpan.FromHours(1), TimeSpan.Zero, TimeSpan.Zero, Monday, Utc, TimeSpan.FromHours(1));

        slots.Should().HaveCount(4);
        slots[0].StartUtc.TimeOfDay.Should().Be(new TimeSpan(10, 0, 0));
        slots[^1].StartUtc.TimeOfDay.Should().Be(new TimeSpan(13, 0, 0));
    }

    [Fact]
    public void CalculateAvailableSlots_EmployeeScheduleNarrowerThanLocationHours_ClipsToEmployeeSchedule()
    {
        var slots = AvailabilityCalculator.CalculateAvailableSlots(
            OpenAllDay(new TimeOnly(8, 0), new TimeOnly(18, 0)),
            [Schedule(DayOfWeek.Monday, new TimeOnly(10, 0), new TimeOnly(14, 0))], null,
            [], TimeSpan.FromHours(1), TimeSpan.Zero, TimeSpan.Zero, Monday, Utc, TimeSpan.FromHours(1));

        slots.Should().HaveCount(4);
        slots[0].StartUtc.TimeOfDay.Should().Be(new TimeSpan(10, 0, 0));
    }

    [Fact]
    public void CalculateAvailableSlots_LocationAndEmployeeWindowsDoNotOverlap_ReturnsEmpty()
    {
        var slots = AvailabilityCalculator.CalculateAvailableSlots(
            OpenAllDay(new TimeOnly(8, 0), new TimeOnly(10, 0)),
            [Schedule(DayOfWeek.Monday, new TimeOnly(14, 0), new TimeOnly(18, 0))], null,
            [], TimeSpan.FromMinutes(30), TimeSpan.Zero, TimeSpan.Zero, Monday, Utc, TimeSpan.FromMinutes(30));

        slots.Should().BeEmpty();
    }

    [Fact]
    public void CalculateAvailableSlots_OccupiedWindow_ExcludesOverlappingCandidate()
    {
        var occupied = new[] { TimeSlot.Create(ToUtc(10, 0), ToUtc(10, 30)).Value };

        var slots = AvailabilityCalculator.CalculateAvailableSlots(
            OpenAllDay(new TimeOnly(9, 0), new TimeOnly(12, 0)),
            [Schedule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0))], null,
            occupied, TimeSpan.FromMinutes(30), TimeSpan.Zero, TimeSpan.Zero, Monday, Utc, TimeSpan.FromMinutes(30));

        slots.Should().HaveCount(5);
        slots.Select(s => s.StartUtc.TimeOfDay).Should().NotContain(new TimeSpan(10, 0, 0));
    }

    [Fact]
    public void CalculateAvailableSlots_NewServiceBufferAfterExtendsBlockingIntoOccupiedWindow_ExcludesOtherwiseFreeCandidate()
    {
        var occupied = new[] { TimeSlot.Create(ToUtc(10, 0), ToUtc(10, 30)).Value };
        var locationHours = OpenAllDay(new TimeOnly(9, 0), new TimeOnly(12, 0));
        var schedule = new[] { Schedule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0)) };

        var withoutBuffer = AvailabilityCalculator.CalculateAvailableSlots(
            locationHours, schedule, null, occupied, TimeSpan.FromMinutes(30), TimeSpan.Zero, TimeSpan.Zero,
            Monday, Utc, TimeSpan.FromMinutes(15));
        withoutBuffer.Select(s => s.StartUtc.TimeOfDay).Should().Contain(new TimeSpan(9, 15, 0));

        var withBuffer = AvailabilityCalculator.CalculateAvailableSlots(
            locationHours, schedule, null, occupied, TimeSpan.FromMinutes(30), TimeSpan.Zero, TimeSpan.FromMinutes(20),
            Monday, Utc, TimeSpan.FromMinutes(15));
        withBuffer.Select(s => s.StartUtc.TimeOfDay).Should().NotContain(new TimeSpan(9, 15, 0));
    }

    [Fact]
    public void CalculateAvailableSlots_CandidateExtendingPastWindowEnd_IsExcluded()
    {
        var slots = AvailabilityCalculator.CalculateAvailableSlots(
            OpenAllDay(new TimeOnly(9, 0), new TimeOnly(9, 45)),
            [Schedule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(9, 45))], null,
            [], TimeSpan.FromMinutes(30), TimeSpan.Zero, TimeSpan.Zero, Monday, Utc, TimeSpan.FromMinutes(30));

        slots.Should().ContainSingle();
        slots[0].StartUtc.TimeOfDay.Should().Be(new TimeSpan(9, 0, 0));
    }

    [Fact]
    public void CalculateAvailableSlots_SplitShiftRecurringSchedule_GeneratesSlotsInBothWindows()
    {
        var slots = AvailabilityCalculator.CalculateAvailableSlots(
            OpenAllDay(new TimeOnly(9, 0), new TimeOnly(19, 0)),
            [
                Schedule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(13, 0)),
                Schedule(DayOfWeek.Monday, new TimeOnly(15, 0), new TimeOnly(19, 0))
            ],
            null, [], TimeSpan.FromHours(1), TimeSpan.Zero, TimeSpan.Zero, Monday, Utc, TimeSpan.FromHours(1));

        slots.Should().HaveCount(8);
        slots.Select(s => s.StartUtc.TimeOfDay).Should().NotContain(new TimeSpan(13, 0, 0));
    }

    [Fact]
    public void CalculateAvailableSlots_NonUtcTimeZone_ConvertsLocalHoursToCorrectUtcInstant()
    {
        var newYork = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        var januaryMonday = new DateOnly(2026, 1, 5); // EST (UTC-5), well clear of any DST transition

        var slots = AvailabilityCalculator.CalculateAvailableSlots(
            OpenAllDay(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            [Schedule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(10, 0))], null,
            [], TimeSpan.FromMinutes(30), TimeSpan.Zero, TimeSpan.Zero, januaryMonday, newYork, TimeSpan.FromMinutes(30));

        slots[0].StartUtc.Should().Be(new DateTime(2026, 1, 5, 14, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void CalculateAvailableSlots_NonPositiveGranularity_ThrowsArgumentOutOfRangeException()
    {
        var act = () => AvailabilityCalculator.CalculateAvailableSlots(
            OpenAllDay(new TimeOnly(9, 0), new TimeOnly(17, 0)),
            [Schedule(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0))], null,
            [], TimeSpan.FromMinutes(30), TimeSpan.Zero, TimeSpan.Zero, Monday, Utc, TimeSpan.Zero);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}