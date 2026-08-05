using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Bookings.Commands.CancelBooking;

internal sealed class CancelBookingCommandHandler(
    IBookingRepository bookingRepository, IApplicationDbContext dbContext, IUnitOfWork unitOfWork)
    : ICommandHandler<CancelBookingCommand>
{
    public async Task<Result> Handle(CancelBookingCommand command, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking is null)
            return Result.Failure(ApplicationErrors.Booking.NotFound);

        var providedToken = SecurityToken.FromExisting(command.Token ?? string.Empty);
        if (!booking.CancellationToken.Matches(providedToken))
            return Result.Failure(ApplicationErrors.Booking.InvalidManagementToken);

        var deadlineHours = await dbContext.Organizations
            .Where(o => o.Id == booking.OrganizationId)
            .Select(o => o.CancellationDeadlineHours)
            .FirstAsync(cancellationToken);

        if (booking.TimeSlot.StartUtc - DateTime.UtcNow < TimeSpan.FromHours(deadlineHours))
            return Result.Failure(ApplicationErrors.Booking.CancellationDeadlinePassed);

        var cancelResult = booking.Cancel(command.Reason, DateTime.UtcNow);
        if (cancelResult.IsFailure)
            return cancelResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}