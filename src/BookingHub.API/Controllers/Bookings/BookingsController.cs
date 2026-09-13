using BookingHub.API.Common;
using BookingHub.Application.Features.Bookings.Commands.MarkCompleted;
using BookingHub.Application.Features.Bookings.Commands.MarkNoShow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.API.Controllers.Bookings;

[Authorize]
[Route("api/v1/organizations/{organizationId:guid}/locations/{locationId:guid}/employees/{employeeId:guid}/bookings/{bookingId:guid}")]
public sealed class BookingsController(IDispatcher dispatcher) : ApiControllerBase
{
    [HttpPost("complete")]
    public async Task<IActionResult> MarkCompleted(
        Guid organizationId, Guid locationId, Guid employeeId, Guid bookingId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new MarkCompletedCommand(organizationId, locationId, employeeId, bookingId), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("no-show")]
    public async Task<IActionResult> MarkNoShow(
        Guid organizationId, Guid locationId, Guid employeeId, Guid bookingId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new MarkNoShowCommand(organizationId, locationId, employeeId, bookingId), cancellationToken);
        return HandleResult(result);
    }
}