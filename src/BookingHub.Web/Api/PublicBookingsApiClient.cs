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
            ? await response.Content.ReadFromJsonAsync<IReadOnlyList<AvailableSlotResponse>>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }

    public async Task<BookingCreatedResponse?> CreateBookingAsync(
        string organizationSlug, CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync(
            $"api/v1/public/{organizationSlug}/bookings", request, ApiJsonOptions.Default, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<BookingCreatedResponse>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }

    public async Task<PublicBookingDetailsResponse?> GetBookingAsync(string organizationSlug, Guid bookingId, string? token, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"api/v1/public/{organizationSlug}/bookings/{bookingId}?token={token}", cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<PublicBookingDetailsResponse>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }

    public async Task<bool> CancelBookingAsync(string organizationSlug, Guid bookingId, string? token, string? reason, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync(
            $"api/v1/public/{organizationSlug}/bookings/{bookingId}/cancel",
            new CancelBookingRequest(token, reason), ApiJsonOptions.Default, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<BookingCreatedResponse?> RescheduleBookingAsync(string organizationSlug, Guid bookingId, string? token, DateTime newStartUtc, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync(
            $"api/v1/public/{organizationSlug}/bookings/{bookingId}/reschedule",
            new RescheduleBookingRequest(token, newStartUtc), ApiJsonOptions.Default, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<BookingCreatedResponse>(ApiJsonOptions.Default, cancellationToken)
            : null;
    }
}