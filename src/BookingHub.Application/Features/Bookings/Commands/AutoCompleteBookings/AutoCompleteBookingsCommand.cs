using BookingHub.Application.Common.Messaging;

namespace BookingHub.Application.Features.Bookings.Commands.AutoCompleteBookings;

public sealed record AutoCompleteBookingsCommand : ICommand<int>;