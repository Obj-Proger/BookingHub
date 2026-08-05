using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Bookings;

internal sealed record AvailabilityContext(
    Location Location, Service Service, Employee Employee, EmployeeLocationAssignment? Assignment,
    IReadOnlyList<RecurringSchedule> RecurringSchedule, ScheduleException? ExceptionForDate,
    IReadOnlyList<TimeSlot> OccupiedWindows, TimeZoneInfo TimeZone);

internal static class AvailabilityContextLoader
{
    public static async Task<Result<AvailabilityContext>> LoadAsync(
        IApplicationDbContext dbContext, Guid organizationId, Guid locationId, Guid employeeId, Guid serviceId,
        DateOnly date, CancellationToken cancellationToken, Guid? excludeBookingId = null)
    {
        var location = await dbContext.Locations
            .FirstOrDefaultAsync(l => l.Id == locationId && l.OrganizationId == organizationId, cancellationToken);
        if (location is null)
            return Result.Failure<AvailabilityContext>(ApplicationErrors.Location.NotFound);

        var service = await dbContext.Services
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.OrganizationId == organizationId, cancellationToken);
        if (service is null)
            return Result.Failure<AvailabilityContext>(ApplicationErrors.Service.NotFound);

        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(e => e.Id == employeeId && e.OrganizationId == organizationId, cancellationToken);
        if (employee is null || !employee.IsBookable)
            return Result.Failure<AvailabilityContext>(ApplicationErrors.Employee.NotFound);

        var assignment = await dbContext.EmployeeLocationAssignments
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.LocationId == locationId && a.IsActive, cancellationToken);

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(location.TimeZone);

        IReadOnlyList<RecurringSchedule> recurringSchedule = [];
        ScheduleException? exceptionForDate = null;
        var occupiedWindows = new List<TimeSlot>();

        if (assignment is not null)
        {
            recurringSchedule = await dbContext.RecurringSchedules
                .Where(s => s.EmployeeLocationAssignmentId == assignment.Id)
                .ToListAsync(cancellationToken);

            exceptionForDate = await dbContext.ScheduleExceptions
                .FirstOrDefaultAsync(e => e.EmployeeLocationAssignmentId == assignment.Id && e.Date == date, cancellationToken);

            var rangeStartUtc = date.AddDays(-1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var rangeEndUtc = date.AddDays(2).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

            var occupiedRaw = await (
                from b in dbContext.Bookings
                join s in dbContext.Services on b.ServiceId equals s.Id
                where b.EmployeeId == employeeId && b.Status == BookingStatus.Confirmed
                    && b.Id != (excludeBookingId ?? Guid.Empty)
                    && b.TimeSlot.StartUtc < rangeEndUtc && b.TimeSlot.EndUtc > rangeStartUtc
                select new { b.TimeSlot.StartUtc, b.TimeSlot.EndUtc, s.BufferBefore, s.BufferAfter })
                .ToListAsync(cancellationToken);

            occupiedWindows = occupiedRaw
                .Select(w => TimeSlot.Create(w.StartUtc - w.BufferBefore, w.EndUtc + w.BufferAfter).Value)
                .ToList();
        }

        return new AvailabilityContext(location, service, employee, assignment, recurringSchedule, exceptionForDate, occupiedWindows, timeZone);
    }
}
