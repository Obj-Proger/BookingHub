using BookingHub.Application.Common.Messaging;

namespace BookingHub.Application.Features.Bookings.Commands.ConfirmBooking;

/// <summary>Anonymous by design — reached via the token link sent after <c>CreateBookingCommand</c>.</summary>
public sealed record ConfirmBookingCommand(Guid BookingId, string? Token) : ICommand;