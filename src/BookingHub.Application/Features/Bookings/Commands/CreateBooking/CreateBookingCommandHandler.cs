using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Notifications;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings.DTOs;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Bookings.Commands.CreateBooking;

internal sealed class CreateBookingCommandHandler(
    IApplicationDbContext dbContext,
    IClientRepository clientRepository,
    IBookingRepository bookingRepository,
    IEmailService emailService,
    ISmsService smsService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateBookingCommand, BookingCreatedResponse>
{
    private static readonly TimeSpan SlotGranularity = TimeSpan.FromMinutes(15);

    public async Task<Result<BookingCreatedResponse>> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        if (command.StartUtc.Kind != DateTimeKind.Utc)
            return Result.Failure<BookingCreatedResponse>(DomainErrors.TimeSlot.NotUtc);

        var phoneResult = PhoneNumber.Create(command.Phone);
        if (phoneResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(phoneResult.Error);

        Email? email = null;
        if (!string.IsNullOrWhiteSpace(command.ClientEmail))
        {
            var emailResult = Email.Create(command.ClientEmail);
            if (emailResult.IsFailure)
                return Result.Failure<BookingCreatedResponse>(emailResult.Error);
            email = emailResult.Value;
        }

        var clientContact = ClientContact.Create(phoneResult.Value, command.ClientName, email);

        var bookingResult = await BookingSlotBuilder.TryCreatePendingBookingAsync(
            dbContext, command.OrganizationId, command.LocationId, command.EmployeeId, command.ServiceId,
            command.StartUtc, clientContact, BookingSource.Public, recurringSeriesId: null, SlotGranularity, cancellationToken);
        if (bookingResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(bookingResult.Error);

        var booking = bookingResult.Value;

        var client = await clientRepository.GetByPhoneAsync(phoneResult.Value, cancellationToken);
        if (client is null)
        {
            client = Client.Create(phoneResult.Value, command.ClientName, email);
            clientRepository.Add(client);
        }
        else
        {
            client.UpdateContactInfo(command.ClientName, email);
        }

        booking.LinkClient(client.Id);
        bookingRepository.Add(booking);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await SendConfirmationAsync(booking, clientContact, cancellationToken);

        return new BookingCreatedResponse(booking.Id, booking.TimeSlot.StartUtc, booking.TimeSlot.EndUtc, booking.Status);
    }

    private async Task SendConfirmationAsync(Booking booking, ClientContact clientContact, CancellationToken cancellationToken)
    {
        var confirmationLink = $"/bookings/{booking.Id}/confirm?token={booking.ConfirmationToken.Value}";

        if (clientContact.Email is not null)
        {
            await emailService.SendAsync(
                new EmailMessage(booking.OrganizationId, clientContact.Email.Value, "Confirm your booking", confirmationLink),
                cancellationToken);
        }

        await smsService.SendAsync(
            new SmsMessage(booking.OrganizationId, clientContact.Phone.Value, $"Confirm your booking: {confirmationLink}"),
            cancellationToken);
    }
}