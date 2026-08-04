using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings.DTOs;
using BookingHub.Domain.Enums;
using BookingHub.Domain.Services;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Bookings.Queries.GetAvailableSlots;

internal sealed class GetAvailableSlotsQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetAvailableSlotsQuery, IReadOnlyList<AvailableSlotResponse>>
{
    private static readonly TimeSpan SlotGranularity = TimeSpan.FromMinutes(15);

    public async Task<Result<IReadOnlyList<AvailableSlotResponse>>> Handle(
        GetAvailableSlotsQuery query, CancellationToken cancellationToken)
    {
        var location = await dbContext.Locations
            .FirstOrDefaultAsync(l => l.Id == query.LocationId && l.OrganizationId == query.OrganizationId, cancellationToken);
        if (location is null)
            return Result.Failure<IReadOnlyList<AvailableSlotResponse>>(ApplicationErrors.Location.NotFound);

        var service = await dbContext.Services
            .FirstOrDefaultAsync(s => s.Id == query.ServiceId && s.OrganizationId == query.OrganizationId, cancellationToken);
        if (service is null)
            return Result.Failure<IReadOnlyList<AvailableSlotResponse>>(ApplicationErrors.Service.NotFound);

        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(e => e.Id == query.EmployeeId && e.OrganizationId == query.OrganizationId, cancellationToken);
        if (employee is null || !employee.IsBookable)
            return Result.Failure<IReadOnlyList<AvailableSlotResponse>>(ApplicationErrors.Employee.NotFound);

        var assignment = await dbContext.EmployeeLocationAssignments
            .FirstOrDefaultAsync(a => a.EmployeeId == query.EmployeeId && a.LocationId == query.LocationId && a.IsActive, cancellationToken);
        if (assignment is null)
            return new List<AvailableSlotResponse>(); // this employee simply doesn't work at this location — not an error

        var recurringSchedule = await dbContext.RecurringSchedules
            .Where(s => s.EmployeeLocationAssignmentId == assignment.Id)
            .ToListAsync(cancellationToken);

        var exceptionForDate = await dbContext.ScheduleExceptions
            .FirstOrDefaultAsync(e => e.EmployeeLocationAssignmentId == assignment.Id && e.Date == query.Date, cancellationToken);

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(location.TimeZone);
        var searchRangeStartUtc = query.Date.AddDays(-1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var searchRangeEndUtc = query.Date.AddDays(2).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var occupiedRaw = await (
            from b in dbContext.Bookings
            join s in dbContext.Services on b.ServiceId equals s.Id
            where b.EmployeeId == query.EmployeeId
                && b.Status == BookingStatus.Confirmed
                && b.TimeSlot.StartUtc < searchRangeEndUtc
                && b.TimeSlot.EndUtc > searchRangeStartUtc
            select new { b.TimeSlot.StartUtc, b.TimeSlot.EndUtc, s.BufferBefore, s.BufferAfter })
            .ToListAsync(cancellationToken);

        // Expansion by each existing booking's own service buffers happens here, in memory,
        // after materialization — TimeSlot.Create is domain code, EF Core cannot translate it to SQL.
        var occupiedWindows = occupiedRaw
            .Select(w => TimeSlot.Create(w.StartUtc - w.BufferBefore, w.EndUtc + w.BufferAfter).Value)
            .ToList();

        var availableSlots = AvailabilityCalculator.CalculateAvailableSlots(
            location.WorkingHours, recurringSchedule, exceptionForDate, occupiedWindows,
            service.Duration, service.BufferBefore, service.BufferAfter, query.Date, timeZone, SlotGranularity);

        return availableSlots.Select(s => new AvailableSlotResponse(s.StartUtc, s.EndUtc)).ToList();
    }
}