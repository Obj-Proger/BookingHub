using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Bookings.Commands.AutoCompleteBookings;

internal sealed class AutoCompleteBookingsCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
: ICommandHandler<AutoCompleteBookingsCommand, int>
{
    public async Task<Result<int>> Handle(AutoCompleteBookingsCommand command, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var bookings = await bookingRepository.GetAwaitingReviewBookingsPastAutoCompleteWindowAsync(utcNow, cancellationToken);

        var completedCount = 0;
        foreach (var booking in bookings)
        {
            if (booking.Complete(utcNow).IsSuccess)
                completedCount++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return completedCount;
    }
}