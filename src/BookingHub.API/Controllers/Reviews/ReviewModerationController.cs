using BookingHub.API.Common;
using BookingHub.Application.Features.Reviews.Commands.HideReview;
using BookingHub.Application.Features.Reviews.Commands.UnhideReview;
using BookingHub.Application.Features.Reviews.Queries.GetLocationReviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.API.Controllers.Reviews;

[Authorize]
[Route("api/v1/organizations/{organizationId:guid}/locations/{locationId:guid}/reviews")]
public sealed class ReviewModerationController(IDispatcher dispatcher) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetForLocation(
        Guid organizationId, Guid locationId, [FromQuery] int skip, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        var result = await dispatcher.Send(new GetLocationReviewsQuery(organizationId, locationId, skip, take), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{reviewId:guid}/hide")]
    public async Task<IActionResult> Hide(Guid organizationId, Guid locationId, Guid reviewId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new HideReviewCommand(organizationId, locationId, reviewId), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{reviewId:guid}/unhide")]
    public async Task<IActionResult> Unhide(Guid organizationId, Guid locationId, Guid reviewId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new UnhideReviewCommand(organizationId, locationId, reviewId), cancellationToken);
        return HandleResult(result);
    }
}