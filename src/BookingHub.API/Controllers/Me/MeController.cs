using BookingHub.API.Common;
using BookingHub.Application.Features.Organizations.Queries.GetMyOrganizationMemberships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.API.Controllers.Me;

[Authorize]
[Route("api/v1/me")]
public sealed class MeController(IDispatcher dispatcher) : ApiControllerBase
{
    [HttpGet("organizations")]
    public async Task<IActionResult> GetOrganizations(CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetMyOrganizationMembershipsQuery(), cancellationToken);
        return HandleResult(result);
    }
}