using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Bookings.Commands.ExpirePendingBookings;

internal sealed class ExpirePendingBookingsCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<ExpirePendingBookingsCommand, int>
{
    public async Task<Result<int>> Handle(ExpirePendingBookingsCommand command, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var bookings = await bookingRepository.GetPendingBookingsPastConfirmationWindowAsync(utcNow, cancellationToken);

        var expiredCount = 0;
        foreach (var booking in bookings)
        {
            if (booking.Expire(utcNow).IsSuccess)
                expiredCount++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return expiredCount;
    }
}