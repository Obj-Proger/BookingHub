using BookingHub.Domain.Entities;

namespace BookingHub.Application.Common.Persistence;

public interface IReviewRepository
{
    void Add(Review review);
    Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken cancellationToken);

    /// <param name="locationId">Filtered alongside <paramref name="reviewId"/> — same rule as
    /// every other location-scoped moderation action in this project.</param>
    Task<Review?> GetByIdAsync(Guid locationId, Guid reviewId, CancellationToken cancellationToken);
}