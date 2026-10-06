using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Notifications;
using BookingHub.Application.Common.Persistence;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Bookings.Commands.ConfirmBooking;

internal sealed class ConfirmBookingCommandHandler(
    IBookingRepository bookingRepository, IUnitOfWork unitOfWork,
    IWebLinkBuilder linkBuilder, IEmailService emailService, ISmsService smsService)
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

        if (booking.RecurringSeriesId is not null)
        {
            var siblings = await bookingRepository.GetPendingSiblingsInSeriesAsync(
                booking.RecurringSeriesId.Value, booking.Id, cancellationToken);

            foreach (var sibling in siblings)
                sibling.Confirm(DateTime.UtcNow);
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Failure(ApplicationErrors.Booking.SlotNotAvailable);
        }

        var manageLink = linkBuilder.Build($"/book/{command.OrganizationSlug}/manage/{booking.Id}?token={booking.CancellationToken.Value}");
        var body = $"Your booking is confirmed. Manage it (cancel/reschedule) here: {manageLink}";

        if (booking.ClientContact.Email is not null)
            await emailService.SendAsync(new EmailMessage(booking.OrganizationId, booking.ClientContact.Email.Value, "Booking confirmed", body), cancellationToken);

        await smsService.SendAsync(new SmsMessage(booking.OrganizationId, booking.ClientContact.Phone.Value, body), cancellationToken);

        return Result.Success();
    }
}