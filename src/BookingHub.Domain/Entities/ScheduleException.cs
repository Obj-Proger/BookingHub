using BookingHub.Domain.Enums;

namespace BookingHub.Domain.Entities;

/// <summary>
/// A one-off override to an employee's recurring schedule on a specific date —
/// either a full day off or hours different from the usual recurring pattern.
/// </summary>
public sealed class ScheduleException : BaseEntity
{
    public Guid EmployeeLocationAssignmentId { get; private set; }
    public DateOnly Date { get; private set; }
    public ScheduleExceptionType Type { get; private set; }
    public TimeOnly? ModifiedStartTime { get; private set; }
    public TimeOnly? ModifiedEndTime { get; private set; }

    private ScheduleException(
        Guid id, Guid employeeLocationAssignmentId, DateOnly date, ScheduleExceptionType type,
        TimeOnly? modifiedStartTime, TimeOnly? modifiedEndTime)
        : base(id)
    {
        EmployeeLocationAssignmentId = employeeLocationAssignmentId;
        Date = date;
        Type = type;
        ModifiedStartTime = modifiedStartTime;
        ModifiedEndTime = modifiedEndTime;
    }

    private ScheduleException()
    {
    }

    /// <summary>Creates an exception marking the employee unavailable for the entire day.</summary>
    public static Result<ScheduleException> CreateDayOff(Guid employeeLocationAssignmentId, DateOnly date)
    {
        var assignmentIdResult = ValidateAssignmentId(employeeLocationAssignmentId);
        if (assignmentIdResult.IsFailure)
            return Result.Failure<ScheduleException>(assignmentIdResult.Error);

        return new ScheduleException(Guid.CreateVersion7(), employeeLocationAssignmentId, date, ScheduleExceptionType.DayOff, null, null);
    }

    /// <summary>Creates an exception with hours that differ from the employee's recurring schedule.</summary>
    public static Result<ScheduleException> CreateModifiedHours(
        Guid employeeLocationAssignmentId, DateOnly date, TimeOnly modifiedStartTime, TimeOnly modifiedEndTime)
    {
        var assignmentIdResult = ValidateAssignmentId(employeeLocationAssignmentId);
        if (assignmentIdResult.IsFailure)
            return Result.Failure<ScheduleException>(assignmentIdResult.Error);

        if (modifiedStartTime >= modifiedEndTime)
            return Result.Failure<ScheduleException>(DomainErrors.ScheduleException.StartNotBeforeEnd);

        return new ScheduleException(
            Guid.CreateVersion7(), employeeLocationAssignmentId, date, ScheduleExceptionType.ModifiedHours, modifiedStartTime, modifiedEndTime);
    }

    private static Result<Guid> ValidateAssignmentId(Guid employeeLocationAssignmentId) =>
        Guard.NotEmpty(employeeLocationAssignmentId, "ScheduleException.EmployeeLocationAssignmentIdEmpty", "EmployeeLocationAssignmentId");
}