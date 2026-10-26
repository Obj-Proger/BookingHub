using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface ISchedulesApiClient
{
    Task<ApiResult<AssignmentScheduleResponse>> GetAssignmentScheduleAsync(
        Guid organizationId, Guid locationId, Guid assignmentId, CancellationToken cancellationToken);

    Task<ApiResult<IReadOnlyList<LocationAssignmentResponse>>> GetLocationAssignmentsAsync(
        Guid organizationId, Guid locationId, CancellationToken cancellationToken);

    Task<ApiResult<RecurringScheduleCreatedResponse>> CreateRecurringAsync(
        Guid organizationId, Guid locationId, Guid assignmentId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime,
        CancellationToken cancellationToken);

    Task<ApiResult> RescheduleRecurringAsync(
        Guid organizationId, Guid locationId, Guid recurringScheduleId, TimeOnly startTime, TimeOnly endTime,
        CancellationToken cancellationToken);

    Task<ApiResult> RemoveRecurringAsync(Guid organizationId, Guid locationId, Guid recurringScheduleId, CancellationToken cancellationToken);

    Task<ApiResult<ScheduleExceptionCreatedResponse>> CreateDayOffAsync(
        Guid organizationId, Guid locationId, Guid assignmentId, DateOnly date, CancellationToken cancellationToken);

    Task<ApiResult<ScheduleExceptionCreatedResponse>> CreateModifiedHoursAsync(
        Guid organizationId, Guid locationId, Guid assignmentId, DateOnly date, TimeOnly startTime, TimeOnly endTime,
        CancellationToken cancellationToken);

    Task<ApiResult> RemoveExceptionAsync(Guid organizationId, Guid locationId, Guid scheduleExceptionId, CancellationToken cancellationToken);
}