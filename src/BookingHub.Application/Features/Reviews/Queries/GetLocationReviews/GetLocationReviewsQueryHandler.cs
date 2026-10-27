using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Reviews.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Reviews.Queries.GetLocationReviews;

internal sealed class GetLocationReviewsQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetLocationReviewsQuery, IReadOnlyList<ReviewModerationResponse>>
{
    private const int MaxPageSize = 100;

    public async Task<Result<IReadOnlyList<ReviewModerationResponse>>> Handle(
        GetLocationReviewsQuery query, CancellationToken cancellationToken)
    {
        var locationExists = await dbContext.Locations
            .AnyAsync(l => l.Id == query.LocationId && l.OrganizationId == query.OrganizationId, cancellationToken);

        if (!locationExists)
            return Result.Failure<IReadOnlyList<ReviewModerationResponse>>(ApplicationErrors.Location.NotFound);

        var skip = Math.Max(query.Skip, 0);
        var take = Math.Clamp(query.Take, 1, MaxPageSize);

        // Review ids are UUIDv7, so descending id order is newest first.
        var reviews = await (
            from r in dbContext.Reviews
            join e in dbContext.Employees on r.EmployeeId equals e.Id
            join b in dbContext.Bookings on r.BookingId equals b.Id
            join s in dbContext.Services on b.ServiceId equals s.Id
            where r.OrganizationId == query.OrganizationId && r.LocationId == query.LocationId
            orderby r.Id descending
            select new ReviewModerationResponse(
                r.Id, e.Id, e.FullName, s.Name, b.TimeSlot.StartUtc, r.Rating, r.Comment, r.IsHidden))
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return reviews;
    }
}