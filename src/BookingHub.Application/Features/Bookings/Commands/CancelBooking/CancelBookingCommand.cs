using BookingHub.Application.Common.Messaging;

namespace BookingHub.Application.Features.Bookings.Commands.CancelBooking;

/// <summary>Anonymous by design — reached via the management link sent with the booking confirmation.</summary>
public sealed record CancelBookingCommand(Guid BookingId, string? Token, string? Reason) : ICommand;