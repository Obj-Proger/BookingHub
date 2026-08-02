namespace BookingHub.Domain.Tests.Entities;

public class ScheduleExceptionTests
{
    private static readonly Guid ValidAssignmentId = Guid.CreateVersion7();
    private static readonly DateOnly SampleDate = new(2026, 12, 25);

    [Fact]
    public void CreateDayOff_ValidAssignmentId_Succeeds()
    {
        var result = ScheduleException.CreateDayOff(ValidAssignmentId, SampleDate);

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(ScheduleExceptionType.DayOff);
        result.Value.ModifiedStartTime.Should().BeNull();
        result.Value.ModifiedEndTime.Should().BeNull();
    }

    [Fact]
    public void CreateDayOff_EmptyAssignmentId_FailsWithValidationError()
    {
        var result = ScheduleException.CreateDayOff(Guid.Empty, SampleDate);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void CreateModifiedHours_ValidTimes_Succeeds()
    {
        var result = ScheduleException.CreateModifiedHours(
            ValidAssignmentId, SampleDate, new TimeOnly(10, 0), new TimeOnly(14, 0));

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(ScheduleExceptionType.ModifiedHours);
        result.Value.ModifiedStartTime.Should().Be(new TimeOnly(10, 0));
        result.Value.ModifiedEndTime.Should().Be(new TimeOnly(14, 0));
    }

    [Fact]
    public void CreateModifiedHours_EmptyAssignmentId_FailsWithValidationError()
    {
        var result = ScheduleException.CreateModifiedHours(
            Guid.Empty, SampleDate, new TimeOnly(10, 0), new TimeOnly(14, 0));

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void CreateModifiedHours_StartNotBeforeEnd_FailsWithStartNotBeforeEndError()
    {
        var result = ScheduleException.CreateModifiedHours(
            ValidAssignmentId, SampleDate, new TimeOnly(14, 0), new TimeOnly(10, 0));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.ScheduleException.StartNotBeforeEnd);
    }
}