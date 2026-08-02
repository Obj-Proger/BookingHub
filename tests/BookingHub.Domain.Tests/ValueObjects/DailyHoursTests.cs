namespace BookingHub.Domain.Tests.ValueObjects;

public class DailyHoursTests
{
    [Fact]
    public void CreateClosed_AlwaysSucceeds()
    {
        var dailyHours = DailyHours.CreateClosed(DayOfWeek.Sunday);

        dailyHours.IsClosed.Should().BeTrue();
        dailyHours.OpenTime.Should().BeNull();
        dailyHours.CloseTime.Should().BeNull();
    }

    [Fact]
    public void CreateOpen_ValidHours_Succeeds()
    {
        var result = DailyHours.CreateOpen(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0));

        result.IsSuccess.Should().BeTrue();
        result.Value.IsClosed.Should().BeFalse();
    }

    [Fact]
    public void CreateOpen_OpenTimeNotBeforeCloseTime_FailsWithOpenNotBeforeCloseError()
    {
        var result = DailyHours.CreateOpen(DayOfWeek.Monday, new TimeOnly(18, 0), new TimeOnly(9, 0));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.DailyHours.OpenNotBeforeClose);
    }
}