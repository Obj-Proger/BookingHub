namespace BookingHub.Web.Api.Contracts;

public enum ScheduleExceptionType
{
    DayOff = 1,
    ModifiedHours = 2
}

public sealed record RecurringScheduleResponse(Guid RecurringScheduleId, DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);

public sealed record ScheduleExceptionResponse(
    Guid ScheduleExceptionId, DateOnly Date, ScheduleExceptionType Type, TimeOnly? ModifiedStartTime, TimeOnly? ModifiedEndTime);

public sealed record AssignmentScheduleResponse(
    Guid AssignmentId, Guid EmployeeId, string EmployeeName,
    IReadOnlyList<RecurringScheduleResponse> RecurringSchedules,
    IReadOnlyList<ScheduleExceptionResponse> Exceptions);

public sealed record LocationAssignmentResponse(Guid AssignmentId, Guid EmployeeId, string EmployeeName);

public sealed record RecurringScheduleCreatedResponse(Guid RecurringScheduleId);

public sealed record ScheduleExceptionCreatedResponse(Guid ScheduleExceptionId);

public sealed record CreateRecurringScheduleRequest(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);

public sealed record RescheduleRecurringScheduleRequest(TimeOnly NewStartTime, TimeOnly NewEndTime);

public sealed record CreateDayOffScheduleExceptionRequest(DateOnly Date);

public sealed record CreateModifiedHoursScheduleExceptionRequest(DateOnly Date, TimeOnly ModifiedStartTime, TimeOnly ModifiedEndTime);