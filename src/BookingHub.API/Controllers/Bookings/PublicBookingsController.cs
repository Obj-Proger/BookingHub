using BookingHub.API.Common;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Bookings.Commands.CancelBooking;
using BookingHub.Application.Features.Bookings.Commands.ConfirmBooking;
using BookingHub.Application.Features.Bookings.Commands.CreateBooking;
using BookingHub.Application.Features.Bookings.Commands.CreateRecurringBookingSeries;
using BookingHub.Application.Features.Bookings.Commands.RescheduleBooking;
using BookingHub.Application.Features.Bookings.Queries.GetAvailableSlots;
using BookingHub.Application.Features.Bookings.Queries.GetPublicBookingByToken;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BookingHub.API.Controllers.Bookings;

[Route("api/v1/public/{organizationSlug}")]
public sealed class PublicBookingsController(IDispatcher dispatcher, ICurrentTenant currentTenant) : PublicApiControllerBase(currentTenant)
{
    [EnableRateLimiting("public-read")]
    [HttpGet("locations/{locationId:guid}/employees/{employeeId:guid}/services/{serviceId:guid}/availability")]
    public async Task<IActionResult> GetAvailableSlots(
        Guid locationId, Guid employeeId, Guid serviceId, [FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new GetAvailableSlotsQuery(OrganizationId, locationId, employeeId, serviceId, date), cancellationToken);
        return HandleResult(result);
    }

    [EnableRateLimiting("public-read")]
    [HttpGet("bookings/{bookingId:guid}")]
    public async Task<IActionResult> Get(Guid bookingId, [FromQuery] string? token, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new GetPublicBookingByTokenQuery(bookingId, token), cancellationToken);
        return HandleResult(result);
    }

    [EnableRateLimiting("public-write")]
    [HttpPost("bookings")]
    public async Task<IActionResult> Create(string organizationSlug, CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new CreateBookingCommand(
                OrganizationId, organizationSlug, request.LocationId, request.EmployeeId, request.ServiceId, request.StartUtc,
                request.Phone, request.ClientName, request.ClientEmail),
            cancellationToken);
        return HandleResult(result);
    }

    [EnableRateLimiting("public-write")]
    [HttpPost("bookings/series")]
    public async Task<IActionResult> CreateSeries(string organizationSlug, CreateRecurringBookingSeriesRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(
            new CreateRecurringBookingSeriesCommand(
                OrganizationId, organizationSlug, request.LocationId, request.EmployeeId, request.ServiceId, request.FirstStartUtc,
                request.IntervalWeeks, request.OccurrenceCount, request.Phone, request.ClientName, request.ClientEmail),
            cancellationToken);
        return HandleResult(result);
    }

    [EnableRateLimiting("public-write")]
    [HttpPost("bookings/{bookingId:guid}/confirm")]
    public async Task<IActionResult> Confirm(string organizationSlug, Guid bookingId, ConfirmBookingRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new ConfirmBookingCommand(bookingId, organizationSlug, request.Token), cancellationToken);
        return HandleResult(result);
    }

    [EnableRateLimiting("public-write")]
    [HttpPost("bookings/{bookingId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid bookingId, CancelBookingRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new CancelBookingCommand(bookingId, request.Token, request.Reason), cancellationToken);
        return HandleResult(result);
    }

    [EnableRateLimiting("public-write")]
    [HttpPost("bookings/{bookingId:guid}/reschedule")]
    public async Task<IActionResult> Reschedule(Guid bookingId, RescheduleBookingRequest request, CancellationToken cancellationToken)
    {
        var result = await dispatcher.Send(new RescheduleBookingCommand(bookingId, request.Token, request.NewStartUtc), cancellationToken);
        return HandleResult(result);
    }
}