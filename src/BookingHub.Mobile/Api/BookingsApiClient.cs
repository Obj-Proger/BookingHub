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

    public Task<bool> CompleteAsync(Guid organizationId, Guid locationId, Guid employeeId, Guid bookingId, CancellationToken cancellationToken) =>
        PostActionAsync(organizationId, locationId, employeeId, bookingId, "complete", cancellationToken);

    public Task<bool> MarkNoShowAsync(Guid organizationId, Guid locationId, Guid employeeId, Guid bookingId, CancellationToken cancellationToken) =>
        PostActionAsync(organizationId, locationId, employeeId, bookingId, "no-show", cancellationToken);

    private async Task<bool> PostActionAsync(
        Guid organizationId, Guid locationId, Guid employeeId, Guid bookingId, string action, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsync(
            $"api/v1/organizations/{organizationId}/locations/{locationId}/employees/{employeeId}/bookings/{bookingId}/{action}",
            content: null, cancellationToken);

        return response.IsSuccessStatusCode;
    }
}