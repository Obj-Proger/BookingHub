using BookingHub.API.Common;
using BookingHub.API.RateLimiting;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Waitlist.Commands.ConfirmWaitlistOffer;
using BookingHub.Application.Features.Waitlist.Commands.JoinWaitlist;
using BookingHub.Application.Features.Waitlist.Commands.LeaveWaitlist;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BookingHub.API.Controllers.Waitlist;

[Route("api/v1/public/{organizationSlug}/waitlist")]
public sealed class PublicWaitlistController(IDispatcher dispatcher, ICurrentTenant currentTenant) : PublicApiControllerBase(currentTenant)
{
    [EnableRateLimiting(RateLimitPolicies.PublicWrite)]
    [HttpPost]
    public async Task<IActionResult> Join(JoinWaitlistRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new JoinWaitlistCommand(
                OrganizationId, request.LocationId, request.EmployeeId, request.ServiceId,
                request.DesiredStartUtc, request.DesiredEndUtc, request.Phone, request.ClientName, request.ClientEmail),
            cancellationToken);
        return HandleResult(result);
    }

    [EnableRateLimiting(RateLimitPolicies.PublicWrite)]
    [HttpPost("{waitlistEntryId:guid}/confirm")]
    public async Task<IActionResult> ConfirmOffer(Guid waitlistEntryId, ConfirmWaitlistOfferRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new ConfirmWaitlistOfferCommand(waitlistEntryId, request.Token), cancellationToken);
        return HandleResult(result);
    }

    [EnableRateLimiting(RateLimitPolicies.PublicWrite)]
    [HttpPost("{waitlistEntryId:guid}/leave")]
    public async Task<IActionResult> Leave(Guid waitlistEntryId, LeaveWaitlistRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new LeaveWaitlistCommand(waitlistEntryId, request.Token), cancellationToken);
        return HandleResult(result);
    }
}