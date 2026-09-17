using BookingHub.API.Common;
using BookingHub.Application.Features.Bookings.Commands.MarkCompleted;
using BookingHub.Application.Features.Bookings.Commands.MarkNoShow;
using BookingHub.Application.Features.Bookings.Queries.GetEmployeeBookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.API.Controllers.Bookings;

[Authorize]
[Route("api/v1/organizations/{organizationId:guid}/locations/{locationId:guid}/employees/{employeeId:guid}")]
public sealed class BookingsController(IDispatcher dispatcher) : ApiControllerBase
{
    [HttpGet("bookings")]
    public async Task<IActionResult> GetSchedule(
        Guid organizationId, Guid locationId, Guid employeeId, [FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetEmployeeBookingsQuery(organizationId, locationId, employeeId, date), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("bookings/{bookingId:guid}/complete")]
    public async Task<IActionResult> MarkCompleted(
        Guid organizationId, Guid locationId, Guid employeeId, Guid bookingId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new MarkCompletedCommand(organizationId, locationId, employeeId, bookingId), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("bookings/{bookingId:guid}/no-show")]
    public async Task<IActionResult> MarkNoShow(
        Guid organizationId, Guid locationId, Guid employeeId, Guid bookingId, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new MarkNoShowCommand(organizationId, locationId, employeeId, bookingId), cancellationToken);
        return HandleResult(result);
    }
}