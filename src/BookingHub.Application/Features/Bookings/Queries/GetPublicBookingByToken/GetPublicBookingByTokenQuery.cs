using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Bookings.DTOs;

namespace BookingHub.Application.Features.Bookings.Queries.GetPublicBookingByToken;

/// <summary>Anonymous by design — reached via the management link sent with the booking confirmation.</summary>
public sealed record GetPublicBookingByTokenQuery(Guid BookingId, string? Token) : IQuery<PublicBookingDetailsResponse>;