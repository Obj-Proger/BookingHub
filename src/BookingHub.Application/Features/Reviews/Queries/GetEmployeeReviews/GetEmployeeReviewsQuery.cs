using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Reviews.DTOs;

namespace BookingHub.Application.Features.Reviews.Queries.GetEmployeeReviews;

/// <summary>Anonymous by design — powers the public booking page's review display (Vision Document, §5.5).</summary>
public sealed record GetEmployeeReviewsQuery(Guid OrganizationId, Guid EmployeeId) : IQuery<IReadOnlyList<ReviewResponse>>;