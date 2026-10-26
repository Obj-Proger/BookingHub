using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Employees.DTOs;

public sealed record RecurringScheduleResponse(Guid RecurringScheduleId, DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);

public sealed record ScheduleExceptionResponse(
    Guid ScheduleExceptionId, DateOnly Date, ScheduleExceptionType Type, TimeOnly? ModifiedStartTime, TimeOnly? ModifiedEndTime);

/// <param name="Exceptions">Dated from the previous UTC day onward, so that "today" is covered in every time zone.</param>
public sealed record AssignmentScheduleResponse(
    Guid AssignmentId, Guid EmployeeId, string EmployeeName,
    IReadOnlyList<RecurringScheduleResponse> RecurringSchedules,
    IReadOnlyList<ScheduleExceptionResponse> Exceptions);