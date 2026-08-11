using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Notifications;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings.DTOs;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Bookings.Commands.CreateRecurringBookingSeries;

internal sealed class CreateRecurringBookingSeriesCommandHandler(
    IApplicationDbContext dbContext,
    IClientRepository clientRepository,
    IBookingRepository bookingRepository,
    IEmailService emailService,
    ISmsService smsService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRecurringBookingSeriesCommand, RecurringBookingSeriesCreatedResponse>
{
    private static readonly TimeSpan SlotGranularity = TimeSpan.FromMinutes(15);

    public async Task<Result<RecurringBookingSeriesCreatedResponse>> Handle(
        CreateRecurringBookingSeriesCommand command, CancellationToken cancellationToken)
    {
        if (command.FirstStartUtc.Kind != DateTimeKind.Utc)
            return Result.Failure<RecurringBookingSeriesCreatedResponse>(DomainErrors.TimeSlot.NotUtc);

        var phoneResult = PhoneNumber.Create(command.Phone);
        if (phoneResult.IsFailure)
            return Result.Failure<RecurringBookingSeriesCreatedResponse>(phoneResult.Error);

        Email? email = null;
        if (!string.IsNullOrWhiteSpace(command.ClientEmail))
        {
            var emailResult = Email.Create(command.ClientEmail);
            if (emailResult.IsFailure)
                return Result.Failure<RecurringBookingSeriesCreatedResponse>(emailResult.Error);
            email = emailResult.Value;
        }

        var clientContact = ClientContact.Create(phoneResult.Value, command.ClientName, email);
        var seriesId = Guid.CreateVersion7();

        var createdBookings = new List<Booking>();
        var skippedStarts = new List<DateTime>();

        for (var occurrence = 0; occurrence < command.OccurrenceCount; occurrence++)
        {
            var occurrenceStartUtc = command.FirstStartUtc.AddDays(7 * command.IntervalWeeks * occurrence);

            var bookingResult = await BookingSlotBuilder.TryCreatePendingBookingAsync(
                dbContext, command.OrganizationId, command.LocationId, command.EmployeeId, command.ServiceId,
                occurrenceStartUtc, clientContact, BookingSource.Public, seriesId, SlotGranularity, cancellationToken);

            if (bookingResult.IsSuccess)
                createdBookings.Add(bookingResult.Value);
            else
                skippedStarts.Add(occurrenceStartUtc);
        }

        if (createdBookings.Count == 0)
            return Result.Failure<RecurringBookingSeriesCreatedResponse>(ApplicationErrors.Booking.NoOccurrenceAvailable);

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

        foreach (var booking in createdBookings)
        {
            booking.LinkClient(client.Id);
            bookingRepository.Add(booking);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await SendConfirmationAsync(createdBookings[0], clientContact, createdBookings.Count, cancellationToken);

        return new RecurringBookingSeriesCreatedResponse(
            seriesId,
            createdBookings.Select(b => new BookingCreatedResponse(b.Id, b.TimeSlot.StartUtc, b.TimeSlot.EndUtc, b.Status)).ToList(),
            skippedStarts);
    }

    private async Task SendConfirmationAsync(Booking firstBooking, ClientContact clientContact, int occurrenceCount, CancellationToken cancellationToken)
    {
        var confirmationLink = $"/bookings/{firstBooking.Id}/confirm?token={firstBooking.ConfirmationToken.Value}";
        var body = $"Confirm your {occurrenceCount}-visit series: {confirmationLink}";

        if (clientContact.Email is not null)
            await emailService.SendAsync(new EmailMessage(firstBooking.OrganizationId, clientContact.Email.Value, "Confirm your bookings", body), cancellationToken);

        await smsService.SendAsync(new SmsMessage(firstBooking.OrganizationId, clientContact.Phone.Value, body), cancellationToken);
    }
}