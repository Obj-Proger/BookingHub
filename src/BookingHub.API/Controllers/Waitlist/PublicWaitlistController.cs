using BookingHub.API.Common;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Waitlist.Commands.ConfirmWaitlistOffer;
using BookingHub.Application.Features.Waitlist.Commands.JoinWaitlist;
using BookingHub.Application.Features.Waitlist.Commands.LeaveWaitlist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.API.Controllers.Waitlist;

[Route("api/v1/public/{organizationSlug}")]
public sealed class PublicWaitlistController(IDispatcher dispatcher, ICurrentTenant currentTenant) : PublicApiControllerBase(currentTenant)
{
    [HttpPost("waitlist")]
    public async Task<IActionResult> Join(JoinWaitlistRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new JoinWaitlistCommand(
                OrganizationId, request.LocationId, request.EmployeeId, request.ServiceId,
                request.DesiredStartUtc, request.DesiredEndUtc, request.Phone, request.ClientName, request.ClientEmail),
            cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// Not nested under api/v1/public/{organizationSlug} — the two actions here are reached by a
/// guest holding a link with only a WaitlistEntryId and a token, the same as booking
/// confirmation/cancellation; no organization context is needed or available at this point.
/// </summary>
[Route("api/v1/public/waitlist/{waitlistEntryId:guid}")]
[AllowAnonymous]
public sealed class WaitlistOffersController(IDispatcher dispatcher) : ApiControllerBase
{
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmOffer(Guid waitlistEntryId, ConfirmWaitlistOfferRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new ConfirmWaitlistOfferCommand(waitlistEntryId, request.Token), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("leave")]
    public async Task<IActionResult> Leave(Guid waitlistEntryId, LeaveWaitlistRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new LeaveWaitlistCommand(waitlistEntryId, request.Token), cancellationToken);
        return HandleResult(result);
    }
}