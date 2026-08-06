using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Bookings.Commands.MarkCompleted;

internal sealed class MarkCompletedCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<MarkCompletedCommand>
{
    public async Task<Result> Handle(MarkCompletedCommand command, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(
            command.OrganizationId, command.LocationId, command.EmployeeId, command.BookingId, cancellationToken);
        if (booking is null)
            return Result.Failure(ApplicationErrors.Booking.NotFound);

        var completeResult = booking.Complete(DateTime.UtcNow);
        if (completeResult.IsFailure)
            return completeResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}