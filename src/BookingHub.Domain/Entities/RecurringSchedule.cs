namespace BookingHub.Domain.Entities;

/// <summary>
/// A recurring weekly working-hours block for an employee at a specific location.
/// Multiple entries may share the same day of week to represent a split shift
/// (e.g. 09:00–13:00 and 15:00–19:00) — non-overlap between sibling entries is
/// validated by the Application layer, which has visibility into all of them at once.
/// </summary>
public sealed class RecurringSchedule : BaseEntity
{
    public Guid EmployeeLocationAssignmentId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    private RecurringSchedule(Guid id, Guid employeeLocationAssignmentId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
        : base(id)
    {
        EmployeeLocationAssignmentId = employeeLocationAssignmentId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }

    private RecurringSchedule()
    {
    }

    public static Result<RecurringSchedule> Create(
        Guid employeeLocationAssignmentId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        var assignmentIdResult = Guard.NotEmpty(
            employeeLocationAssignmentId, "RecurringSchedule.EmployeeLocationAssignmentIdEmpty", "EmployeeLocationAssignmentId");
        if (assignmentIdResult.IsFailure)
            return Result.Failure<RecurringSchedule>(assignmentIdResult.Error);

        if (startTime >= endTime)
            return Result.Failure<RecurringSchedule>(DomainErrors.RecurringSchedule.StartNotBeforeEnd);

        return new RecurringSchedule(Guid.CreateVersion7(), employeeLocationAssignmentId, dayOfWeek, startTime, endTime);
    }

    public Result Reschedule(TimeOnly newStartTime, TimeOnly newEndTime)
    {
        if (newStartTime >= newEndTime)
            return Result.Failure(DomainErrors.RecurringSchedule.StartNotBeforeEnd);

        StartTime = newStartTime;
        EndTime = newEndTime;
        return Result.Success();
    }
}