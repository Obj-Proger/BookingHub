using BookingHub.API.Common;
using BookingHub.Application.Features.Services.Commands.CreateLocationServiceOverride;
using BookingHub.Application.Features.Services.Commands.CreateService;
using BookingHub.Application.Features.Services.Commands.DeleteLocationServiceOverride;
using BookingHub.Application.Features.Services.Commands.RenameService;
using BookingHub.Application.Features.Services.Commands.UpdateLocationServiceOverridePrice;
using BookingHub.Application.Features.Services.Commands.UpdateServiceBuffers;
using BookingHub.Application.Features.Services.Commands.UpdateServiceColor;
using BookingHub.Application.Features.Services.Commands.UpdateServiceDuration;
using BookingHub.Application.Features.Services.Commands.UpdateServicePricing;
using BookingHub.Application.Features.Services.Queries.GetService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.API.Controllers.Services;

[Authorize]
[Route("api/v1/organizations/{organizationId:guid}")]
public sealed class ServicesController(IDispatcher dispatcher) : ApiControllerBase
{
    // Service (organization-wide)

    [HttpPost("services")]
    public async Task<IActionResult> Create(Guid organizationId, CreateServiceRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new CreateServiceCommand(
                organizationId, request.Name, request.Duration, request.BasePriceAmount, request.BasePriceCurrency,
                request.BufferBefore, request.BufferAfter, request.Color),
            cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("services/{serviceId:guid}")]
    public async Task<IActionResult> Get(Guid organizationId, Guid serviceId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetServiceQuery(organizationId, serviceId), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("services/{serviceId:guid}/name")]
    public async Task<IActionResult> Rename(Guid organizationId, Guid serviceId, RenameServiceRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new RenameServiceCommand(organizationId, serviceId, request.NewName), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("services/{serviceId:guid}/pricing")]
    public async Task<IActionResult> UpdatePricing(
        Guid organizationId, Guid serviceId, UpdateServicePricingRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new UpdateServicePricingCommand(organizationId, serviceId, request.NewAmount, request.NewCurrency), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("services/{serviceId:guid}/duration")]
    public async Task<IActionResult> UpdateDuration(
        Guid organizationId, Guid serviceId, UpdateServiceDurationRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new UpdateServiceDurationCommand(organizationId, serviceId, request.NewDuration), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("services/{serviceId:guid}/buffers")]
    public async Task<IActionResult> UpdateBuffers(
        Guid organizationId, Guid serviceId, UpdateServiceBuffersRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new UpdateServiceBuffersCommand(organizationId, serviceId, request.NewBufferBefore, request.NewBufferAfter), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("services/{serviceId:guid}/color")]
    public async Task<IActionResult> UpdateColor(
        Guid organizationId, Guid serviceId, UpdateServiceColorRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new UpdateServiceColorCommand(organizationId, serviceId, request.NewColor), cancellationToken);
        return HandleResult(result);
    }

    // Location-specific price overrides (location-scoped)

    [HttpPost("locations/{locationId:guid}/service-overrides")]
    public async Task<IActionResult> CreateOverride(
        Guid organizationId, Guid locationId, CreateLocationServiceOverrideRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new CreateLocationServiceOverrideCommand(
                organizationId, locationId, request.ServiceId, request.OverridePriceAmount, request.OverridePriceCurrency),
            cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("locations/{locationId:guid}/service-overrides/{overrideId:guid}")]
    public async Task<IActionResult> UpdateOverridePrice(
        Guid organizationId, Guid locationId, Guid overrideId, UpdateLocationServiceOverridePriceRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new UpdateLocationServiceOverridePriceCommand(organizationId, locationId, overrideId, request.NewAmount, request.NewCurrency),
            cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("locations/{locationId:guid}/service-overrides/{overrideId:guid}")]
    public async Task<IActionResult> DeleteOverride(Guid organizationId, Guid locationId, Guid overrideId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new DeleteLocationServiceOverrideCommand(organizationId, locationId, overrideId), cancellationToken);
        return HandleResult(result);
    }
}