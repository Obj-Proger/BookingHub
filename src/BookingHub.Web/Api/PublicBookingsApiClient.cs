using System.Net.Http.Json;
using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

internal sealed class PublicBookingsApiClient(HttpClient httpClient) : IPublicBookingsApiClient
{
    public async Task<IReadOnlyList<AvailableSlotResponse>?> GetAvailableSlotsAsync(
        string organizationSlug, Guid locationId, Guid employeeId, Guid serviceId, DateOnly date, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(
            $"api/v1/public/{organizationSlug}/locations/{locationId}/employees/{employeeId}/services/{serviceId}/availability?date={date:yyyy-MM-dd}",
            cancellationToken);

        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<IReadOnlyList<AvailableSlotResponse>>(cancellationToken: cancellationToken)
            : null;
    }

    public async Task<BookingCreatedResponse?> CreateBookingAsync(
        string organizationSlug, CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync($"api/v1/public/{organizationSlug}/bookings", request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<BookingCreatedResponse>(cancellationToken: cancellationToken)
            : null;
    }
}