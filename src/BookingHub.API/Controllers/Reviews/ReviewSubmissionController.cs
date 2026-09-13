using BookingHub.API.Common;
using BookingHub.Application.Features.Reviews.Commands.SubmitReview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.API.Controllers.Reviews;

/// <summary>Same reasoning as WaitlistOffersController — the guest holds only a BookingId and
/// its (reused) management token, no organization context.</summary>
[Route("api/v1/public/bookings/{bookingId:guid}/review")]
[AllowAnonymous]
public sealed class ReviewSubmissionController(IDispatcher dispatcher) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit(Guid bookingId, SubmitReviewRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new SubmitReviewCommand(bookingId, request.Token, request.Rating, request.Comment), cancellationToken);
        return HandleResult(result);
    }
}