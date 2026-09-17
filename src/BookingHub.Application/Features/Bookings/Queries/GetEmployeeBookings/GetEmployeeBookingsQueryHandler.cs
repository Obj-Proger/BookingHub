using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Bookings.Queries.GetEmployeeBookings;

internal sealed class GetEmployeeBookingsQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetEmployeeBookingsQuery, IReadOnlyList<EmployeeBookingResponse>>
{
    public async Task<Result<IReadOnlyList<EmployeeBookingResponse>>> Handle(
        GetEmployeeBookingsQuery query, CancellationToken cancellationToken)
    {
        var locationTimeZoneId = await dbContext.Locations
            .Where(l => l.Id == query.LocationId && l.OrganizationId == query.OrganizationId)
            .Select(l => l.TimeZone)
            .FirstOrDefaultAsync(cancellationToken);
        if (locationTimeZoneId is null)
            return Result.Failure<IReadOnlyList<EmployeeBookingResponse>>(ApplicationErrors.Location.NotFound);

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(locationTimeZoneId);
        var dayStartUtc = TimeZoneInfo.ConvertTimeToUtc(query.Date.ToDateTime(TimeOnly.MinValue), timeZone);
        var dayEndUtc = TimeZoneInfo.ConvertTimeToUtc(query.Date.AddDays(1).ToDateTime(TimeOnly.MinValue), timeZone);

        var bookings = await (
            from b in dbContext.Bookings
            join s in dbContext.Services on b.ServiceId equals s.Id
            where b.OrganizationId == query.OrganizationId && b.LocationId == query.LocationId
                && b.EmployeeId == query.EmployeeId
                && b.TimeSlot.StartUtc >= dayStartUtc && b.TimeSlot.StartUtc < dayEndUtc
            orderby b.TimeSlot.StartUtc
            select new EmployeeBookingResponse(
                b.Id, b.TimeSlot.StartUtc, b.TimeSlot.EndUtc, s.Name, b.ClientContact.Name, b.ClientContact.Phone.Value, b.Status))
            .ToListAsync(cancellationToken);

        return bookings;
    }
}