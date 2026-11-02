using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IPublicBookingsApiClient
{
    Task<ApiResult<IReadOnlyList<AvailableSlotResponse>>> GetAvailableSlotsAsync(
        string organizationSlug, Guid locationId, Guid employeeId, Guid serviceId, DateOnly date, CancellationToken cancellationToken);

    Task<ApiResult<BookingCreatedResponse>> CreateBookingAsync(
        string organizationSlug, CreateBookingRequest request, CancellationToken cancellationToken);

    /// <param name="token">The confirmation token from the link texted after booking.</param>
    Task<ApiResult> ConfirmBookingAsync(string organizationSlug, Guid bookingId, string? token, CancellationToken cancellationToken);

    /// <param name="token">The management token from the link texted after confirmation.</param>
    Task<ApiResult<PublicBookingDetailsResponse>> GetBookingAsync(
        string organizationSlug, Guid bookingId, string? token, CancellationToken cancellationToken);

    Task<ApiResult> CancelBookingAsync(
        string organizationSlug, Guid bookingId, string? token, string? reason, CancellationToken cancellationToken);

    Task<ApiResult<BookingCreatedResponse>> RescheduleBookingAsync(
        string organizationSlug, Guid bookingId, string? token, DateTime newStartUtc, CancellationToken cancellationToken);
}