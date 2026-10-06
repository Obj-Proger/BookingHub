using BookingHub.Web.Api.Contracts;

namespace BookingHub.Web.Api;

public interface IPublicBookingsApiClient
{
    Task<IReadOnlyList<AvailableSlotResponse>?> GetAvailableSlotsAsync(
        string organizationSlug, Guid locationId, Guid employeeId, Guid serviceId, DateOnly date, CancellationToken cancellationToken);

    Task<BookingCreatedResponse?> CreateBookingAsync(string organizationSlug, CreateBookingRequest request, CancellationToken cancellationToken);
}