using BookingHub.API.Common;
using BookingHub.Application.Features.Clients.Queries.GetClientProfile;
using BookingHub.Application.Features.Clients.Queries.SearchClientByPhone;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.API.Controllers.Clients;

[Authorize]
[Route("api/v1/organizations/{organizationId:guid}/clients")]
public sealed class ClientsController(IDispatcher dispatcher) : ApiControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> SearchByPhone(Guid organizationId, [FromQuery] string? phone, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new SearchClientByPhoneQuery(organizationId, phone), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{clientId:guid}")]
    public async Task<IActionResult> GetProfile(Guid organizationId, Guid clientId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetClientProfileQuery(organizationId, clientId), cancellationToken);
        return HandleResult(result);
    }
}