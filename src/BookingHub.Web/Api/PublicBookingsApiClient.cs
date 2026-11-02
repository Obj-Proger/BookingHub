using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

internal sealed class PublicBookingsApiClient(HttpClient httpClient) : IPublicBookingsApiClient
{
    public Task<ApiResult<IReadOnlyList<AvailableSlotResponse>>> GetAvailableSlotsAsync(
        string organizationSlug, Guid locationId, Guid employeeId, Guid serviceId, DateOnly date, CancellationToken cancellationToken) =>
        httpClient.GetApiResultAsync<IReadOnlyList<AvailableSlotResponse>>(
            $"api/v1/public/{organizationSlug}/locations/{locationId}/employees/{employeeId}/services/{serviceId}/availability?date={date:yyyy-MM-dd}",
            cancellationToken);

    public Task<ApiResult<BookingCreatedResponse>> CreateBookingAsync(
        string organizationSlug, CreateBookingRequest request, CancellationToken cancellationToken) =>
        httpClient.PostApiResultAsync<BookingCreatedResponse>(
            $"api/v1/public/{organizationSlug}/bookings", request, cancellationToken);

    public Task<ApiResult> ConfirmBookingAsync(string organizationSlug, Guid bookingId, string? token, CancellationToken cancellationToken) =>
        httpClient.PostApiResultAsync(
            $"api/v1/public/{organizationSlug}/bookings/{bookingId}/confirm", new ConfirmBookingRequest(token), cancellationToken);

    public Task<ApiResult<PublicBookingDetailsResponse>> GetBookingAsync(
        string organizationSlug, Guid bookingId, string? token, CancellationToken cancellationToken) =>
        httpClient.GetApiResultAsync<PublicBookingDetailsResponse>(
            $"api/v1/public/{organizationSlug}/bookings/{bookingId}?token={Uri.EscapeDataString(token ?? string.Empty)}",
            cancellationToken);

    public Task<ApiResult> CancelBookingAsync(
        string organizationSlug, Guid bookingId, string? token, string? reason, CancellationToken cancellationToken) =>
        httpClient.PostApiResultAsync(
            $"api/v1/public/{organizationSlug}/bookings/{bookingId}/cancel", new CancelBookingRequest(token, reason), cancellationToken);

    public Task<ApiResult<BookingCreatedResponse>> RescheduleBookingAsync(
        string organizationSlug, Guid bookingId, string? token, DateTime newStartUtc, CancellationToken cancellationToken) =>
        httpClient.PostApiResultAsync<BookingCreatedResponse>(
            $"api/v1/public/{organizationSlug}/bookings/{bookingId}/reschedule",
            new RescheduleBookingRequest(token, newStartUtc), cancellationToken);
}