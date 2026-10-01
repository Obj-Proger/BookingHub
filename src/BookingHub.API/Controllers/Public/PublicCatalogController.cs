using BookingHub.API.Common;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Public.Queries.GetPublicEmployees;
using BookingHub.Application.Features.Public.Queries.GetPublicLocations;
using BookingHub.Application.Features.Public.Queries.GetPublicServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BookingHub.API.Controllers.Public;

[EnableRateLimiting("public-read")]
[Route("api/v1/public/{organizationSlug}")]
public sealed class PublicCatalogController(IDispatcher dispatcher, ICurrentTenant currentTenant) : PublicApiControllerBase(currentTenant)
{
    [HttpGet("locations")]
    public async Task<IActionResult> GetLocations(CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetPublicLocationsQuery(OrganizationId), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("locations/{locationId:guid}/services")]
    public async Task<IActionResult> GetServices(Guid locationId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetPublicServicesQuery(OrganizationId, locationId), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("locations/{locationId:guid}/employees")]
    public async Task<IActionResult> GetEmployees(Guid locationId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetPublicEmployeesQuery(OrganizationId, locationId), cancellationToken);
        return HandleResult(result);
    }
}