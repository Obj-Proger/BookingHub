using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Notifications;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Bookings.Commands.MarkCompleted;

internal sealed class MarkCompletedCommandHandler(
    IBookingRepository bookingRepository, IUnitOfWork unitOfWork,
    IWebLinkBuilder linkBuilder, IEmailService emailService, ISmsService smsService)
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

        var reviewLink = linkBuilder.Build($"/book/review/{booking.Id}?token={booking.CancellationToken.Value}");
        var body = $"Thanks for visiting! Mind leaving a quick review? {reviewLink}";

        if (booking.ClientContact.Email is not null)
            await emailService.SendAsync(new EmailMessage(booking.OrganizationId, booking.ClientContact.Email.Value, "How was your visit?", body), cancellationToken);

        await smsService.SendAsync(new SmsMessage(booking.OrganizationId, booking.ClientContact.Phone.Value, body), cancellationToken);

        return Result.Success();
    }
}