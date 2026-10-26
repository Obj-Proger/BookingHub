using BookingHub.Web.Api.Contracts;
using BookingHub.Web.Auth;

namespace BookingHub.Web.Api;

internal sealed class SchedulesApiClient(AuthorizedApiClient authorizedClient) : ISchedulesApiClient
{
    private static string LocationPath(Guid organizationId, Guid locationId) =>
        $"api/v1/organizations/{organizationId}/locations/{locationId}";

    public Task<ApiResult<AssignmentScheduleResponse>> GetAssignmentScheduleAsync(
        Guid organizationId, Guid locationId, Guid assignmentId, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<AssignmentScheduleResponse>(
            $"{LocationPath(organizationId, locationId)}/assignments/{assignmentId}/schedule", cancellationToken);

    public Task<ApiResult<IReadOnlyList<LocationAssignmentResponse>>> GetLocationAssignmentsAsync(
        Guid organizationId, Guid locationId, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<IReadOnlyList<LocationAssignmentResponse>>(
            $"{LocationPath(organizationId, locationId)}/employees", cancellationToken);

    public Task<ApiResult<RecurringScheduleCreatedResponse>> CreateRecurringAsync(
        Guid organizationId, Guid locationId, Guid assignmentId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime,
        CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync<RecurringScheduleCreatedResponse>(
            HttpMethod.Post, $"{LocationPath(organizationId, locationId)}/assignments/{assignmentId}/schedule/recurring",
            new CreateRecurringScheduleRequest(dayOfWeek, startTime, endTime), cancellationToken);

    public Task<ApiResult> RescheduleRecurringAsync(
        Guid organizationId, Guid locationId, Guid recurringScheduleId, TimeOnly startTime, TimeOnly endTime,
        CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Put, $"{LocationPath(organizationId, locationId)}/schedule/recurring/{recurringScheduleId}",
            new RescheduleRecurringScheduleRequest(startTime, endTime), cancellationToken);

    public Task<ApiResult> RemoveRecurringAsync(
        Guid organizationId, Guid locationId, Guid recurringScheduleId, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Delete, $"{LocationPath(organizationId, locationId)}/schedule/recurring/{recurringScheduleId}",
            body: null, cancellationToken);

    public Task<ApiResult<ScheduleExceptionCreatedResponse>> CreateDayOffAsync(
        Guid organizationId, Guid locationId, Guid assignmentId, DateOnly date, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync<ScheduleExceptionCreatedResponse>(
            HttpMethod.Post, $"{LocationPath(organizationId, locationId)}/assignments/{assignmentId}/schedule/exceptions/day-off",
            new CreateDayOffScheduleExceptionRequest(date), cancellationToken);

    public Task<ApiResult<ScheduleExceptionCreatedResponse>> CreateModifiedHoursAsync(
        Guid organizationId, Guid locationId, Guid assignmentId, DateOnly date, TimeOnly startTime, TimeOnly endTime,
        CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync<ScheduleExceptionCreatedResponse>(
            HttpMethod.Post, $"{LocationPath(organizationId, locationId)}/assignments/{assignmentId}/schedule/exceptions/modified-hours",
            new CreateModifiedHoursScheduleExceptionRequest(date, startTime, endTime), cancellationToken);

    public Task<ApiResult> RemoveExceptionAsync(
        Guid organizationId, Guid locationId, Guid scheduleExceptionId, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Delete, $"{LocationPath(organizationId, locationId)}/schedule/exceptions/{scheduleExceptionId}",
            body: null, cancellationToken);
}