using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Bookings.DTOs;

namespace BookingHub.Application.Features.Bookings.Queries.GetLocationBookings;

/// <summary>All bookings of one location on a calendar day (in the location's time zone), across employees.</summary>
public sealed record GetLocationBookingsQuery(Guid OrganizationId, Guid LocationId, DateOnly Date)
    : IQuery<IReadOnlyList<LocationBookingResponse>>, IRequireLocationManagement;