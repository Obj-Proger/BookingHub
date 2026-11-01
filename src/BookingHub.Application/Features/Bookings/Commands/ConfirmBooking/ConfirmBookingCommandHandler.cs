using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Notifications;
using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Bookings.Commands.ConfirmBooking;

internal sealed class ConfirmBookingCommandHandler(
    IBookingRepository bookingRepository, IGuestMessages guestMessages, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : ICommandHandler<ConfirmBookingCommand>
{
    public async Task<Result> Handle(ConfirmBookingCommand command, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking is null)
            return Result.Failure(ApplicationErrors.Booking.NotFound);

        var providedToken = SecurityToken.FromExisting(command.Token ?? string.Empty);
        if (!booking.ConfirmationToken.Matches(providedToken))
            return Result.Failure(ApplicationErrors.Booking.InvalidConfirmationToken);

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var confirmResult = booking.Confirm(utcNow);
        if (confirmResult.IsFailure)
            return confirmResult;

        if (booking.RecurringSeriesId is not null)
        {
            var siblings = await bookingRepository.GetPendingSiblingsInSeriesAsync(
                booking.RecurringSeriesId.Value, booking.Id, cancellationToken);

            foreach (var sibling in siblings)
                sibling.Confirm(utcNow);
        }

        await guestMessages.BookingConfirmedAsync(booking, cancellationToken);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Failure(ApplicationErrors.Booking.SlotNotAvailable);
        }

        return Result.Success();
    }
}