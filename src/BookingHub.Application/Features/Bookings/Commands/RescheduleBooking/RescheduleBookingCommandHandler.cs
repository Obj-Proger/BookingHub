using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings.DTOs;
using BookingHub.Domain.Services;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Bookings.Commands.RescheduleBooking;

internal sealed class RescheduleBookingCommandHandler(
    IBookingRepository bookingRepository, IApplicationDbContext dbContext, IUnitOfWork unitOfWork)
    : ICommandHandler<RescheduleBookingCommand, BookingCreatedResponse>
{
    private static readonly TimeSpan SlotGranularity = TimeSpan.FromMinutes(15);

    public async Task<Result<BookingCreatedResponse>> Handle(RescheduleBookingCommand command, CancellationToken cancellationToken)
    {
        if (command.NewStartUtc.Kind != DateTimeKind.Utc)
            return Result.Failure<BookingCreatedResponse>(DomainErrors.TimeSlot.NotUtc);

        var booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking is null)
            return Result.Failure<BookingCreatedResponse>(ApplicationErrors.Booking.NotFound);

        var providedToken = SecurityToken.FromExisting(command.Token ?? string.Empty);
        if (!booking.CancellationToken.Matches(providedToken))
            return Result.Failure<BookingCreatedResponse>(ApplicationErrors.Booking.InvalidManagementToken);

        var deadlineHours = await dbContext.Organizations
            .Where(o => o.Id == booking.OrganizationId)
            .Select(o => o.CancellationDeadlineHours)
            .FirstAsync(cancellationToken);

        if (booking.TimeSlot.StartUtc - DateTime.UtcNow < TimeSpan.FromHours(deadlineHours))
            return Result.Failure<BookingCreatedResponse>(ApplicationErrors.Booking.CancellationDeadlinePassed);

        var locationTimeZoneId = await dbContext.Locations
            .Where(l => l.Id == booking.LocationId)
            .Select(l => l.TimeZone)
            .FirstAsync(cancellationToken);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(locationTimeZoneId);
        var localDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(command.NewStartUtc, timeZone));

        var contextResult = await AvailabilityContextLoader.LoadAsync(
            dbContext, booking.OrganizationId, booking.LocationId, booking.EmployeeId, booking.ServiceId,
            localDate, cancellationToken, excludeBookingId: booking.Id);
        if (contextResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(contextResult.Error);

        var context = contextResult.Value;

        var availableSlots = AvailabilityCalculator.CalculateAvailableSlots(
            context.Location.WorkingHours, context.RecurringSchedule, context.ExceptionForDate, context.OccupiedWindows,
            context.Service.Duration, context.Service.BufferBefore, context.Service.BufferAfter,
            localDate, context.TimeZone, SlotGranularity);

        if (availableSlots.All(s => s.StartUtc != command.NewStartUtc))
            return Result.Failure<BookingCreatedResponse>(ApplicationErrors.Booking.SlotNotAvailable);

        var newTimeSlotResult = TimeSlot.Create(command.NewStartUtc, command.NewStartUtc + context.Service.Duration);
        if (newTimeSlotResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(newTimeSlotResult.Error);

        var rescheduleResult = booking.Reschedule(newTimeSlotResult.Value, DateTime.UtcNow);
        if (rescheduleResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(rescheduleResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new BookingCreatedResponse(booking.Id, booking.TimeSlot.StartUtc, booking.TimeSlot.EndUtc, booking.Status);
    }
}