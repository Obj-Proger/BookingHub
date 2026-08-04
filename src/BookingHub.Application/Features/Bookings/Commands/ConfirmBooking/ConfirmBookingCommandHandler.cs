using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Bookings.Commands.ConfirmBooking;

internal sealed class ConfirmBookingCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
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

        var confirmResult = booking.Confirm(DateTime.UtcNow);
        if (confirmResult.IsFailure)
            return confirmResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}