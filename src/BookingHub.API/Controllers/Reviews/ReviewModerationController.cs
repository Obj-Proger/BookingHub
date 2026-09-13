using BookingHub.API.Common;
using BookingHub.Application.Features.Reviews.Commands.HideReview;
using BookingHub.Application.Features.Reviews.Commands.UnhideReview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.API.Controllers.Reviews;

[Authorize]
[Route("api/v1/organizations/{organizationId:guid}/locations/{locationId:guid}/reviews/{reviewId:guid}")]
public sealed class ReviewModerationController(IDispatcher dispatcher) : ApiControllerBase
{
    [HttpPost("hide")]
    public async Task<IActionResult> Hide(Guid organizationId, Guid locationId, Guid reviewId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new HideReviewCommand(organizationId, locationId, reviewId), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("unhide")]
    public async Task<IActionResult> Unhide(Guid organizationId, Guid locationId, Guid reviewId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new UnhideReviewCommand(organizationId, locationId, reviewId), cancellationToken);
        return HandleResult(result);
    }
}