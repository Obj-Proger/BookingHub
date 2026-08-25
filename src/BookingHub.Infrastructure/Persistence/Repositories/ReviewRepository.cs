using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Infrastructure.Persistence.Repositories;

internal sealed class ReviewRepository(ApplicationDbContext dbContext) : IReviewRepository
{
    public void Add(Review review) => dbContext.Reviews.Add(review);

    public Task<bool> ExistsForBookingAsync(Guid bookingId, CancellationToken cancellationToken) =>
        dbContext.Reviews.AnyAsync(r => r.BookingId == bookingId, cancellationToken);

    public Task<Review?> GetByIdAsync(Guid locationId, Guid reviewId, CancellationToken cancellationToken) =>
        dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.LocationId == locationId, cancellationToken);
}