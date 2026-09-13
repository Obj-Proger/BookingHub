namespace BookingHub.API.Controllers.Employees;

public sealed record CreateEmployeeRequest(string? FullName);
public sealed record RenameEmployeeRequest(string? NewFullName);
public sealed record SetEmployeeBookableRequest(bool IsBookable);
public sealed record UpdateEmployeePhotoRequest(string? PhotoUrl);
public sealed record AssignEmployeeToLocationRequest(Guid EmployeeId);

/// <param name="StartTime">.NET TimeOnly format ("HH:mm:ss" or "HH:mm") — native System.Text.Json support since .NET 6.</param>
public sealed record CreateRecurringScheduleRequest(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);
public sealed record RescheduleRecurringScheduleRequest(TimeOnly NewStartTime, TimeOnly NewEndTime);

/// <param name="Date">.NET DateOnly format ("yyyy-MM-dd") — same native support as TimeOnly.</param>
public sealed record CreateDayOffScheduleExceptionRequest(DateOnly Date);
public sealed record CreateModifiedHoursScheduleExceptionRequest(DateOnly Date, TimeOnly ModifiedStartTime, TimeOnly ModifiedEndTime);