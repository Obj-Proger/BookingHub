namespace BookingHub.Domain.Tests.ValueObjects;

public class WeeklyHoursTests
{
    [Fact]
    public void Create_AllSevenDaysProvidedOnce_Succeeds()
    {
        var days = Enum.GetValues<DayOfWeek>().Select(DailyHours.CreateClosed);

        var result = WeeklyHours.Create(days);

        result.IsSuccess.Should().BeTrue();
        result.Value.Days.Should().HaveCount(7);
    }

    [Fact]
    public void Create_MissingADay_FailsWithMustCoverAllDaysError()
    {
        var days = Enum.GetValues<DayOfWeek>().Skip(1).Select(DailyHours.CreateClosed);

        var result = WeeklyHours.Create(days);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.WeeklyHours.MustCoverAllDays);
    }

    [Fact]
    public void Create_DuplicateDay_FailsWithMustCoverAllDaysError()
    {
        var days = Enum.GetValues<DayOfWeek>().Take(6)
            .Append(DayOfWeek.Monday)
            .Select(DailyHours.CreateClosed);

        var result = WeeklyHours.Create(days);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.WeeklyHours.MustCoverAllDays);
    }

    [Fact]
    public void For_ReturnsTheMatchingDay()
    {
        var mondayOpen = DailyHours.CreateOpen(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0)).Value;
        var days = Enum.GetValues<DayOfWeek>()
            .Select(day => day == DayOfWeek.Monday ? mondayOpen : DailyHours.CreateClosed(day));

        var weeklyHours = WeeklyHours.Create(days).Value;

        weeklyHours.For(DayOfWeek.Monday).Should().Be(mondayOpen);
    }
}