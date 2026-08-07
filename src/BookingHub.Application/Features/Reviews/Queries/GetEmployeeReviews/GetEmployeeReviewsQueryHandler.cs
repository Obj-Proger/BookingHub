using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Reviews.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Reviews.Queries.GetEmployeeReviews;

internal sealed class GetEmployeeReviewsQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetEmployeeReviewsQuery, IReadOnlyList<ReviewResponse>>
{
    public async Task<Result<IReadOnlyList<ReviewResponse>>> Handle(GetEmployeeReviewsQuery query, CancellationToken cancellationToken)
    {
        var reviews = await dbContext.Reviews
            .Where(r => r.OrganizationId == query.OrganizationId && r.EmployeeId == query.EmployeeId && !r.IsHidden)
            .Select(r => new ReviewResponse(r.Id, r.Rating, r.Comment))
            .ToListAsync(cancellationToken);

        return reviews;
    }
}