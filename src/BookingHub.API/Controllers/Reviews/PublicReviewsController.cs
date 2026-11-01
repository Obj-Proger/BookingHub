using BookingHub.API.Common;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Reviews.Commands.SubmitReview;
using BookingHub.Application.Features.Reviews.Queries.GetEmployeeReviews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BookingHub.API.Controllers.Reviews;

[Route("api/v1/public/{organizationSlug}")]
public sealed class PublicReviewsController(IDispatcher dispatcher, ICurrentTenant currentTenant) : PublicApiControllerBase(currentTenant)
{
    [EnableRateLimiting("public-read")]
    [HttpGet("employees/{employeeId:guid}/reviews")]
    public async Task<IActionResult> GetForEmployee(Guid employeeId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetEmployeeReviewsQuery(OrganizationId, employeeId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>Reached from the link sent once a visit is completed; the booking's management token authenticates it.</summary>
    [EnableRateLimiting("public-write")]
    [HttpPost("bookings/{bookingId:guid}/review")]
    public async Task<IActionResult> Submit(Guid bookingId, SubmitReviewRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new SubmitReviewCommand(bookingId, request.Token, request.Rating, request.Comment), cancellationToken);
        return HandleResult(result);
    }
}