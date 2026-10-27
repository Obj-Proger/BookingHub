using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Reviews.DTOs;

namespace BookingHub.Application.Features.Reviews.Queries.GetLocationReviews;

/// <summary>Every review of a location, hidden ones included, newest first — the moderation view.</summary>
public sealed record GetLocationReviewsQuery(Guid OrganizationId, Guid LocationId, int Skip, int Take)
    : IQuery<IReadOnlyList<ReviewModerationResponse>>, IRequireLocationManagement;