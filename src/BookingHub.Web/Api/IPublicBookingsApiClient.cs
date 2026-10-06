using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IPublicBookingsApiClient
{
    Task<IReadOnlyList<AvailableSlotResponse>?> GetAvailableSlotsAsync(
        string organizationSlug, Guid locationId, Guid employeeId, Guid serviceId, DateOnly date, CancellationToken cancellationToken);

    Task<BookingCreatedResponse?> CreateBookingAsync(string organizationSlug, CreateBookingRequest request, CancellationToken cancellationToken);

    Task<PublicBookingDetailsResponse?> GetBookingAsync(string organizationSlug, Guid bookingId, string? token, CancellationToken cancellationToken);
    Task<bool> CancelBookingAsync(string organizationSlug, Guid bookingId, string? token, string? reason, CancellationToken cancellationToken);
    Task<BookingCreatedResponse?> RescheduleBookingAsync(string organizationSlug, Guid bookingId, string? token, DateTime newStartUtc, CancellationToken cancellationToken);
}