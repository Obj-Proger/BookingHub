using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Bookings.DTOs;

namespace BookingHub.Application.Features.Bookings.Queries.GetEmployeeBookings;

public sealed record GetEmployeeBookingsQuery(Guid OrganizationId, Guid LocationId, Guid EmployeeId, DateOnly Date)
    : IQuery<IReadOnlyList<EmployeeBookingResponse>>, IRequireBookingAccess;