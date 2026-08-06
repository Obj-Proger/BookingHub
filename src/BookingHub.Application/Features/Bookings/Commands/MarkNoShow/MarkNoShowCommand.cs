using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Bookings.Commands.MarkNoShow;

public sealed record MarkNoShowCommand(Guid OrganizationId, Guid LocationId, Guid EmployeeId, Guid BookingId)
    : ICommand, IRequireBookingAccess;