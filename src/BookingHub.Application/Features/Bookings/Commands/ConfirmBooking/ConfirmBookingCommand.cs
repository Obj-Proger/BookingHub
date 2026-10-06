using BookingHub.Application.Common.Messaging;

namespace BookingHub.Application.Features.Bookings.Commands.ConfirmBooking;

/// <summary>Anonymous by design — reached via the confirmation link sent after booking creation.</summary>
public sealed record ConfirmBookingCommand(Guid BookingId, string OrganizationSlug, string? Token) : ICommand;