using BookingHub.API.Common;
using BookingHub.Application.Features.Locations.Commands.CreateLocation;
using BookingHub.Application.Features.Locations.Commands.RenameLocation;
using BookingHub.Application.Features.Locations.Commands.UpdateLocationAddress;
using BookingHub.Application.Features.Locations.Commands.UpdateLocationWorkingHours;
using BookingHub.Application.Features.Locations.Queries.GetLocation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.API.Controllers.Locations;

[Authorize]
[Route("api/v1/organizations/{organizationId:guid}/locations")]
public sealed class LocationsController(IDispatcher dispatcher) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(Guid organizationId, CreateLocationRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new CreateLocationCommand(organizationId, request.Name, request.Address, request.TimeZone, request.WorkingHours), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{locationId:guid}")]
    public async Task<IActionResult> Get(Guid organizationId, Guid locationId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetLocationQuery(organizationId, locationId), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{locationId:guid}/name")]
    public async Task<IActionResult> Rename(Guid organizationId, Guid locationId, RenameLocationRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new RenameLocationCommand(organizationId, locationId, request.NewName), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{locationId:guid}/address")]
    public async Task<IActionResult> UpdateAddress(
        Guid organizationId, Guid locationId, UpdateLocationAddressRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new UpdateLocationAddressCommand(organizationId, locationId, request.NewAddress), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{locationId:guid}/working-hours")]
    public async Task<IActionResult> UpdateWorkingHours(
        Guid organizationId, Guid locationId, UpdateLocationWorkingHoursRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new UpdateLocationWorkingHoursCommand(organizationId, locationId, request.WorkingHours), cancellationToken);
        return HandleResult(result);
    }
}