using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.Services;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Bookings;

/// <summary>
/// Checks availability and builds one Pending Booking for one occurrence — shared by
/// CreateBookingCommand (a single occurrence) and CreateRecurringBookingSeriesCommand
/// (many occurrences, one per loop iteration), so the availability logic exists once.
/// </summary>
internal static class BookingSlotBuilder
{
    public static async Task<Result<Booking>> TryCreatePendingBookingAsync(
        IApplicationDbContext dbContext, Guid organizationId, Guid locationId, Guid employeeId, Guid serviceId,
        DateTime startUtc, ClientContact clientContact, BookingSource source, Guid? recurringSeriesId,
        TimeSpan slotGranularity, CancellationToken cancellationToken)
    {
        var locationTimeZoneId = await dbContext.Locations
            .Where(l => l.Id == locationId && l.OrganizationId == organizationId)
            .Select(l => l.TimeZone)
            .FirstOrDefaultAsync(cancellationToken);
        if (locationTimeZoneId is null)
            return Result.Failure<Booking>(ApplicationErrors.Location.NotFound);

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(locationTimeZoneId);
        var localDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(startUtc, timeZone));

        var contextResult = await AvailabilityContextLoader.LoadAsync(
            dbContext, organizationId, locationId, employeeId, serviceId, localDate, cancellationToken);
        if (contextResult.IsFailure)
            return Result.Failure<Booking>(contextResult.Error);

        var context = contextResult.Value;
        if (context.Assignment is null)
            return Result.Failure<Booking>(ApplicationErrors.Employee.NotAssignedToLocation);

        var availableSlots = AvailabilityCalculator.CalculateAvailableSlots(
            context.Location.WorkingHours, context.RecurringSchedule, context.ExceptionForDate, context.OccupiedWindows,
            context.Service.Duration, context.Service.BufferBefore, context.Service.BufferAfter,
            localDate, context.TimeZone, slotGranularity);

        if (availableSlots.All(s => s.StartUtc != startUtc))
            return Result.Failure<Booking>(ApplicationErrors.Booking.SlotNotAvailable);

        var overrideEntity = await dbContext.LocationServiceOverrides
            .FirstOrDefaultAsync(o => o.LocationId == locationId && o.ServiceId == serviceId, cancellationToken);
        var effectivePrice = overrideEntity?.OverridePrice ?? context.Service.BasePrice;

        var timeSlotResult = TimeSlot.Create(startUtc, startUtc + context.Service.Duration);
        if (timeSlotResult.IsFailure)
            return Result.Failure<Booking>(timeSlotResult.Error);

        return Booking.CreatePending(
            organizationId, locationId, employeeId, serviceId, clientContact, timeSlotResult.Value, effectivePrice,
            source, DateTime.UtcNow, recurringSeriesId);
    }
}