using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IStaffBookingsApiClient
{
    /// <summary>All bookings of the location on a calendar day; for managers.</summary>
    Task<ApiResult<IReadOnlyList<LocationBookingResponse>>> GetLocationDayAsync(
        Guid organizationId, Guid locationId, DateOnly date, CancellationToken cancellationToken);

    Task<ApiResult<IReadOnlyList<EmployeeBookingResponse>>> GetEmployeeDayAsync(
        Guid organizationId, Guid locationId, Guid employeeId, DateOnly date, CancellationToken cancellationToken);

    Task<ApiResult> MarkCompletedAsync(
        Guid organizationId, Guid locationId, Guid employeeId, Guid bookingId, CancellationToken cancellationToken);

    Task<ApiResult> MarkNoShowAsync(
        Guid organizationId, Guid locationId, Guid employeeId, Guid bookingId, CancellationToken cancellationToken);
}