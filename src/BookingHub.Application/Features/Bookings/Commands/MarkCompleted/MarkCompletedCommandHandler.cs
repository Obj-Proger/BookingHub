using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Notifications;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Bookings.Commands.MarkCompleted;

internal sealed class MarkCompletedCommandHandler(
    IBookingRepository bookingRepository, IGuestMessages guestMessages, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : ICommandHandler<MarkCompletedCommand>
{
    public async Task<Result> Handle(MarkCompletedCommand command, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(
            command.OrganizationId, command.LocationId, command.EmployeeId, command.BookingId, cancellationToken);
        if (booking is null)
            return Result.Failure(ApplicationErrors.Booking.NotFound);

        var completeResult = booking.Complete(timeProvider.GetUtcNow().UtcDateTime);
        if (completeResult.IsFailure)
            return completeResult;

        await guestMessages.ReviewRequestedAsync(booking, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}