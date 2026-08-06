using BookingHub.Application.Common.Messaging;

namespace BookingHub.Application.Features.Bookings.Commands.ExpirePendingBookings;

/// <summary>
/// System-triggered — invoked directly by an Infrastructure background job, never exposed
/// through a public API route. No <c>ICurrentUser</c>/authorization marker: there is no
/// caller identity for a scheduled sweep across every organization at once.
/// </summary>
public sealed record ExpirePendingBookingsCommand : ICommand<int>;