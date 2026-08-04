using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IBookingRepository
{
    void Add(Booking booking);

    /// <remarks>Not scoped by OrganizationId — this backs the anonymous guest confirmation
    /// link, which carries only a BookingId; the actual access check is the token, not org membership.</remarks>
    Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken);
}