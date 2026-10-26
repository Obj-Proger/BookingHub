using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Bookings.Queries.GetLocationBookings;

internal sealed class GetLocationBookingsQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetLocationBookingsQuery, IReadOnlyList<LocationBookingResponse>>
{
    public async Task<Result<IReadOnlyList<LocationBookingResponse>>> Handle(
        GetLocationBookingsQuery query, CancellationToken cancellationToken)
    {
        var locationTimeZoneId = await dbContext.Locations
            .Where(l => l.Id == query.LocationId && l.OrganizationId == query.OrganizationId)
            .Select(l => l.TimeZone)
            .FirstOrDefaultAsync(cancellationToken);
        if (locationTimeZoneId is null)
            return Result.Failure<IReadOnlyList<LocationBookingResponse>>(ApplicationErrors.Location.NotFound);

        var (dayStartUtc, dayEndUtc) = LocationDayRange.ToUtc(locationTimeZoneId, query.Date);

        var bookings = await (
            from b in dbContext.Bookings
            join s in dbContext.Services on b.ServiceId equals s.Id
            join e in dbContext.Employees on b.EmployeeId equals e.Id
            where b.OrganizationId == query.OrganizationId && b.LocationId == query.LocationId
                && b.TimeSlot.StartUtc >= dayStartUtc && b.TimeSlot.StartUtc < dayEndUtc
            orderby b.TimeSlot.StartUtc
            select new LocationBookingResponse(
                b.Id, b.EmployeeId, e.FullName, b.TimeSlot.StartUtc, b.TimeSlot.EndUtc,
                s.Name, b.ClientContact.Name, b.ClientContact.Phone.Value, b.Status))
            .ToListAsync(cancellationToken);

        return bookings;
    }
}