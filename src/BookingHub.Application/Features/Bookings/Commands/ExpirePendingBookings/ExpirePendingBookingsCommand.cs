using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Bookings.Commands.ExpirePendingBookings;

/// <summary>Expires pending bookings that were not confirmed in time; sent by a background job.</summary>
public sealed record ExpirePendingBookingsCommand(Guid OrganizationId) : ICommand<int>, ISystemRequest;