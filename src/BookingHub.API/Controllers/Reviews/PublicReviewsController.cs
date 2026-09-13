using BookingHub.API.Common;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Reviews.Queries.GetEmployeeReviews;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.API.Controllers.Reviews;

[Route("api/v1/public/{organizationSlug}")]
public sealed class PublicReviewsController(IDispatcher dispatcher, ICurrentTenant currentTenant) : PublicApiControllerBase(currentTenant)
{
    [HttpGet("employees/{employeeId:guid}/reviews")]
    public async Task<IActionResult> GetForEmployee(Guid employeeId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetEmployeeReviewsQuery(OrganizationId, employeeId), cancellationToken);
        return HandleResult(result);
    }
}