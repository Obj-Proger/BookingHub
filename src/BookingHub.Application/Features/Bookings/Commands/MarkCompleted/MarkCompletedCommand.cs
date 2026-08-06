using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Bookings.Commands.MarkCompleted;

public sealed record MarkCompletedCommand(Guid OrganizationId, Guid LocationId, Guid EmployeeId, Guid BookingId)
    : ICommand, IRequireBookingAccess;