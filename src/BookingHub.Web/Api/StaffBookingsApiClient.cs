using System.Globalization;
using BookingHub.Web.Api.Contracts;
using BookingHub.Web.Auth;

namespace BookingHub.Web.Api;

internal sealed class StaffBookingsApiClient(AuthorizedApiClient authorizedClient) : IStaffBookingsApiClient
{
    private static string LocationPath(Guid organizationId, Guid locationId) =>
        $"api/v1/organizations/{organizationId}/locations/{locationId}";

    private static string DateQuery(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public Task<ApiResult<IReadOnlyList<LocationBookingResponse>>> GetLocationDayAsync(
        Guid organizationId, Guid locationId, DateOnly date, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<IReadOnlyList<LocationBookingResponse>>(
            $"{LocationPath(organizationId, locationId)}/bookings?date={DateQuery(date)}", cancellationToken);

    public Task<ApiResult<IReadOnlyList<EmployeeBookingResponse>>> GetEmployeeDayAsync(
        Guid organizationId, Guid locationId, Guid employeeId, DateOnly date, CancellationToken cancellationToken) =>
        authorizedClient.GetJsonAsync<IReadOnlyList<EmployeeBookingResponse>>(
            $"{LocationPath(organizationId, locationId)}/employees/{employeeId}/bookings?date={DateQuery(date)}", cancellationToken);

    public Task<ApiResult> MarkCompletedAsync(
        Guid organizationId, Guid locationId, Guid employeeId, Guid bookingId, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Post, $"{LocationPath(organizationId, locationId)}/employees/{employeeId}/bookings/{bookingId}/complete",
            body: null, cancellationToken);

    public Task<ApiResult> MarkNoShowAsync(
        Guid organizationId, Guid locationId, Guid employeeId, Guid bookingId, CancellationToken cancellationToken) =>
        authorizedClient.SendJsonAsync(
            HttpMethod.Post, $"{LocationPath(organizationId, locationId)}/employees/{employeeId}/bookings/{bookingId}/no-show",
            body: null, cancellationToken);
}