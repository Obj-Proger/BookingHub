using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings.DTOs;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Bookings.Commands.RescheduleBooking;

internal sealed class RescheduleBookingCommandHandler(
    IBookingRepository bookingRepository,
    IApplicationDbContext dbContext,
    SlotAvailabilityService slotAvailability,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<RescheduleBookingCommand, BookingCreatedResponse>
{
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

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var cancellationDeadline = await dbContext.Organizations
            .Where(o => o.Id == booking.OrganizationId)
            .Select(o => o.CancellationDeadline)
            .FirstAsync(cancellationToken);

        if (booking.TimeSlot.StartUtc - utcNow < cancellationDeadline)
            return Result.Failure<BookingCreatedResponse>(ApplicationErrors.Booking.CancellationDeadlinePassed);

        var slotResult = await slotAvailability.CheckSlotAsync(
            booking.OrganizationId, booking.LocationId, booking.EmployeeId, booking.ServiceId, command.NewStartUtc,
            excludeBookingId: booking.Id, cancellationToken);
        if (slotResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(slotResult.Error);

        // The price stays as booked: rescheduling moves the visit, it does not re-price it.
        var rescheduleResult = booking.Reschedule(slotResult.Value.Slot, utcNow);
        if (rescheduleResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(rescheduleResult.Error);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Failure<BookingCreatedResponse>(ApplicationErrors.Booking.SlotNotAvailable);
        }

        return new BookingCreatedResponse(booking.Id, booking.TimeSlot.StartUtc, booking.TimeSlot.EndUtc, booking.Status);
    }
}