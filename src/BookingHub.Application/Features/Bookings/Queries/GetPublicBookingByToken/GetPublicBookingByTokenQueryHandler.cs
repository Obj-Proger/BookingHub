using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings.DTOs;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Bookings.Queries.GetPublicBookingByToken;

internal sealed class GetPublicBookingByTokenQueryHandler(IBookingRepository bookingRepository, IApplicationDbContext dbContext)
    : IQueryHandler<GetPublicBookingByTokenQuery, PublicBookingDetailsResponse>
{
    public async Task<Result<PublicBookingDetailsResponse>> Handle(GetPublicBookingByTokenQuery query, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(query.BookingId, cancellationToken);
        if (booking is null)
            return Result.Failure<PublicBookingDetailsResponse>(ApplicationErrors.Booking.NotFound);

        var providedToken = SecurityToken.FromExisting(query.Token ?? string.Empty);
        if (!booking.CancellationToken.Matches(providedToken))
            return Result.Failure<PublicBookingDetailsResponse>(ApplicationErrors.Booking.InvalidManagementToken);

        var serviceName = await dbContext.Services
            .Where(s => s.Id == booking.ServiceId)
            .Select(s => s.Name)
            .FirstAsync(cancellationToken);

        return new PublicBookingDetailsResponse(
            booking.Id, booking.LocationId, booking.EmployeeId, booking.ServiceId, serviceName,
            booking.TimeSlot.StartUtc, booking.TimeSlot.EndUtc, booking.Status);
    }
}