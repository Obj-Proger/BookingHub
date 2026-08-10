using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Bookings.Commands.TransitionBookingsToAwaitingReview;

internal sealed class TransitionBookingsToAwaitingReviewCommandHandler(
    IBookingRepository bookingRepository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    : ICommandHandler<TransitionBookingsToAwaitingReviewCommand, int>
{
    public async Task<Result<int>> Handle(TransitionBookingsToAwaitingReviewCommand command, CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var bookings = await bookingRepository.GetConfirmedBookingsWithEndedSlotsAsync(utcNow, cancellationToken);

        var transitionedCount = 0;
        foreach (var booking in bookings)
        {
            if (booking.TransitionToAwaitingReview(utcNow).IsSuccess)
                transitionedCount++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return transitionedCount;
    }
}