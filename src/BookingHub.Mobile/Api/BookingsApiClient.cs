using System.Net.Http.Json;
using BookingHub.Mobile.Api.Contracts;

namespace BookingHub.Mobile.Api;

internal sealed class BookingsApiClient(HttpClient httpClient) : IBookingsApiClient
{
    public async Task<IReadOnlyList<EmployeeBookingResponse>?> GetScheduleAsync(
        Guid organizationId, Guid locationId, Guid employeeId, DateOnly date, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(
            $"api/v1/organizations/{organizationId}/locations/{locationId}/employees/{employeeId}/bookings?date={date:yyyy-MM-dd}",
            cancellationToken);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<IReadOnlyList<EmployeeBookingResponse>>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }
}