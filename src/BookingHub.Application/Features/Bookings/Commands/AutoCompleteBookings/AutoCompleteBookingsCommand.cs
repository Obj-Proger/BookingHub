using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Bookings.Commands.AutoCompleteBookings;

/// <summary>Completes bookings nobody marked within the auto-complete window; sent by a background job.</summary>
public sealed record AutoCompleteBookingsCommand : ICommand<int>, ISystemRequest;