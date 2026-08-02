namespace BookingHub.Domain.Tests.Entities;

public class RecurringScheduleTests
{
    [Fact]
    public void Create_ValidTimes_Succeeds()
    {
        var result = RecurringSchedule.Create(
            Guid.CreateVersion7(), DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0));

        result.IsSuccess.Should().BeTrue();
        result.Value.DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void Create_EmptyAssignmentId_FailsWithValidationError()
    {
        var result = RecurringSchedule.Create(
            Guid.Empty, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0));

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_StartNotBeforeEnd_FailsWithStartNotBeforeEndError()
    {
        var result = RecurringSchedule.Create(
            Guid.CreateVersion7(), DayOfWeek.Monday, new TimeOnly(18, 0), new TimeOnly(9, 0));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.RecurringSchedule.StartNotBeforeEnd);
    }

    [Fact]
    public void Reschedule_ValidTimes_UpdatesStartAndEndTime()
    {
        var schedule = RecurringSchedule.Create(
            Guid.CreateVersion7(), DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0)).Value;

        var result = schedule.Reschedule(new TimeOnly(10, 0), new TimeOnly(19, 0));

        result.IsSuccess.Should().BeTrue();
        schedule.StartTime.Should().Be(new TimeOnly(10, 0));
        schedule.EndTime.Should().Be(new TimeOnly(19, 0));
    }

    [Fact]
    public void Reschedule_StartNotBeforeEnd_FailsAndLeavesTimesUnchanged()
    {
        var schedule = RecurringSchedule.Create(
            Guid.CreateVersion7(), DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0)).Value;

        var result = schedule.Reschedule(new TimeOnly(18, 0), new TimeOnly(9, 0));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.RecurringSchedule.StartNotBeforeEnd);
        schedule.StartTime.Should().Be(new TimeOnly(9, 0));
        schedule.EndTime.Should().Be(new TimeOnly(18, 0));
    }
}