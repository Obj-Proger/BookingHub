using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.Services;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Bookings;

/// <summary>A start time that can be booked right now, with the price it would be booked at.</summary>
internal sealed record BookableSlot(TimeSlot Slot, Money Price);

/// <summary>
/// The single answer to "which slots can be booked": the public slot list and every command that
/// commits to a slot (booking, series, reschedule, accepting a waitlist offer) go through here, so
/// a slot is bookable exactly when the list would offer it.
/// </summary>
internal sealed class SlotAvailabilityService(IApplicationDbContext dbContext, TimeProvider timeProvider)
{
    /// <summary>The step between candidate start times, counted from the start of each working window.</summary>
    public static readonly TimeSpan SlotGranularity = TimeSpan.FromMinutes(15);

    /// <summary>Future slots on a calendar date in the location's own time zone.</summary>
    public async Task<Result<IReadOnlyList<TimeSlot>>> GetAvailableSlotsAsync(
        Guid organizationId, Guid locationId, Guid employeeId, Guid serviceId, DateOnly localDate,
        CancellationToken cancellationToken)
    {
        var contextResult = await LoadContextAsync(
            organizationId, locationId, employeeId, serviceId, _ => localDate, excludeBookingId: null, cancellationToken);
        if (contextResult.IsFailure)
            return Result.Failure<IReadOnlyList<TimeSlot>>(contextResult.Error);

        var context = contextResult.Value;
        if (context.Assignment is null)
            return new List<TimeSlot>();

        return Calculate(context);
    }

    /// <summary>
    /// Checks one chosen start time against the same rules as <see cref="GetAvailableSlotsAsync"/> and
    /// returns the slot with its effective price (the location's override, otherwise the base price).
    /// </summary>
    /// <param name="excludeBookingId">A booking whose own slot must not count as occupied (the one being rescheduled).</param>
    public async Task<Result<BookableSlot>> CheckSlotAsync(
        Guid organizationId, Guid locationId, Guid employeeId, Guid serviceId, DateTime startUtc,
        Guid? excludeBookingId, CancellationToken cancellationToken)
    {
        if (startUtc.Kind != DateTimeKind.Utc)
            return Result.Failure<BookableSlot>(DomainErrors.TimeSlot.NotUtc);

        var contextResult = await LoadContextAsync(
            organizationId, locationId, employeeId, serviceId,
            timeZone => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(startUtc, timeZone)),
            excludeBookingId, cancellationToken);
        if (contextResult.IsFailure)
            return Result.Failure<BookableSlot>(contextResult.Error);

        var context = contextResult.Value;
        if (context.Assignment is null)
            return Result.Failure<BookableSlot>(ApplicationErrors.Employee.NotAssignedToLocation);

        var slot = Calculate(context).FirstOrDefault(s => s.StartUtc == startUtc);
        if (slot is null)
            return Result.Failure<BookableSlot>(ApplicationErrors.Booking.SlotNotAvailable);

        var priceOverride = await dbContext.LocationServiceOverrides
            .FirstOrDefaultAsync(o => o.LocationId == locationId && o.ServiceId == serviceId, cancellationToken);

        return new BookableSlot(slot, priceOverride?.OverridePrice ?? context.Service.BasePrice);
    }

    private List<TimeSlot> Calculate(AvailabilityContext context)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        return AvailabilityCalculator.CalculateAvailableSlots(
                context.Location.WorkingHours, context.RecurringSchedule, context.ExceptionForDate, context.OccupiedWindows,
                context.Service.Duration, context.Service.BufferBefore, context.Service.BufferAfter,
                context.LocalDate, context.TimeZone, SlotGranularity)
            .Where(slot => slot.StartUtc > utcNow)
            .ToList();
    }

    /// <param name="resolveLocalDate">
    /// Picks the calendar date once the location's time zone is known — given directly by the caller,
    /// or derived from a chosen start time.
    /// </param>
    private async Task<Result<AvailabilityContext>> LoadContextAsync(
        Guid organizationId, Guid locationId, Guid employeeId, Guid serviceId,
        Func<TimeZoneInfo, DateOnly> resolveLocalDate, Guid? excludeBookingId, CancellationToken cancellationToken)
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

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(location.TimeZone);
        var localDate = resolveLocalDate(timeZone);

        var assignment = await dbContext.EmployeeLocationAssignments
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.LocationId == locationId && a.IsActive, cancellationToken);
        if (assignment is null)
            return new AvailabilityContext(location, service, null, [], null, [], timeZone, localDate);

        var recurringSchedule = await dbContext.RecurringSchedules
            .Where(s => s.EmployeeLocationAssignmentId == assignment.Id)
            .ToListAsync(cancellationToken);

        var exceptionForDate = await dbContext.ScheduleExceptions
            .FirstOrDefaultAsync(e => e.EmployeeLocationAssignmentId == assignment.Id && e.Date == localDate, cancellationToken);

        // A UTC range wide enough to cover the local day in any time zone.
        var rangeStartUtc = localDate.AddDays(-1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var rangeEndUtc = localDate.AddDays(2).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var occupied = await (
            from b in dbContext.Bookings
            join s in dbContext.Services on b.ServiceId equals s.Id
            where b.EmployeeId == employeeId && b.Status == BookingStatus.Confirmed
                && b.Id != (excludeBookingId ?? Guid.Empty)
                && b.TimeSlot.StartUtc < rangeEndUtc && b.TimeSlot.EndUtc > rangeStartUtc
            select new { b.TimeSlot.StartUtc, b.TimeSlot.EndUtc, s.BufferBefore, s.BufferAfter })
            .ToListAsync(cancellationToken);

        // Each occupied window is widened by the buffers of the service booked in it.
        var occupiedWindows = occupied
            .Select(w => TimeSlot.Create(w.StartUtc - w.BufferBefore, w.EndUtc + w.BufferAfter).Value)
            .ToList();

        return new AvailabilityContext(
            location, service, assignment, recurringSchedule, exceptionForDate, occupiedWindows, timeZone, localDate);
    }

    private sealed record AvailabilityContext(
        Location Location, Service Service, EmployeeLocationAssignment? Assignment,
        IReadOnlyList<RecurringSchedule> RecurringSchedule, ScheduleException? ExceptionForDate,
        IReadOnlyList<TimeSlot> OccupiedWindows, TimeZoneInfo TimeZone, DateOnly LocalDate);
}