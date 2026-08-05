using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Bookings.DTOs;

namespace BookingHub.Application.Features.Bookings.Commands.RescheduleBooking;

/// <summary>Anonymous by design — reached via the same management link as <c>CancelBookingCommand</c>.</summary>
public sealed record RescheduleBookingCommand(Guid BookingId, string? Token, DateTime NewStartUtc)
    : ICommand<BookingCreatedResponse>;