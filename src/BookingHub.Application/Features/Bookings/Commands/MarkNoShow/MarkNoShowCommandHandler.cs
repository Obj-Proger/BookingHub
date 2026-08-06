using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Bookings.Commands.MarkNoShow;

internal sealed class MarkNoShowCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<MarkNoShowCommand>
{
    public async Task<Result> Handle(MarkNoShowCommand command, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(
            command.OrganizationId, command.LocationId, command.EmployeeId, command.BookingId, cancellationToken);
        if (booking is null)
            return Result.Failure(ApplicationErrors.Booking.NotFound);

        var noShowResult = booking.MarkNoShow(DateTime.UtcNow);
        if (noShowResult.IsFailure)
            return noShowResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}