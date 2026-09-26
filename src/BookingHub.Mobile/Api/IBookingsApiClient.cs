using BookingHub.Mobile.Api.Contracts;

namespace BookingHub.Mobile.Api;

public interface IBookingsApiClient
{
    Task<IReadOnlyList<EmployeeBookingResponse>?> GetScheduleAsync(
        Guid organizationId, Guid locationId, Guid employeeId, DateOnly date, CancellationToken cancellationToken);
}