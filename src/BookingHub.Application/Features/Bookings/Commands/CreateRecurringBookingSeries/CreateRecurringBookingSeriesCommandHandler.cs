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
    IWebLinkBuilder linkBuilder,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRecurringBookingSeriesCommand, RecurringBookingSeriesCreatedResponse>
{
    private static readonly TimeSpan SlotGranularity = TimeSpan.FromMinutes(15);

    public async Task<Result<RecurringBookingSeriesCreatedResponse>> Handle(
        CreateRecurringBookingSeriesCommand command, CancellationToken cancellationToken)
    {
        if (command.FirstStartUtc.Kind != DateTimeKind.Utc)
            return Result.Failure<RecurringBookingSeriesCreatedResponse>(DomainErrors.TimeSlot.NotUtc);

        var contactResult = ClientContact.Create(command.Phone, command.ClientName, command.ClientEmail);
        if (contactResult.IsFailure)
            return Result.Failure<RecurringBookingSeriesCreatedResponse>(contactResult.Error);

        var clientContact = contactResult.Value;
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

        var client = await clientRepository.GetByPhoneAsync(clientContact.Phone, cancellationToken);
        if (client is null)
        {
            client = Client.Create(clientContact);
            clientRepository.Add(client);
        }
        else
        {
            client.UpdateContactInfo(clientContact);
        }

        foreach (var booking in createdBookings)
        {
            booking.LinkClient(client.Id);
            bookingRepository.Add(booking);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await SendConfirmationAsync(createdBookings[0], clientContact, createdBookings.Count, command.OrganizationSlug, cancellationToken);

        return new RecurringBookingSeriesCreatedResponse(
            seriesId,
            [.. createdBookings.Select(b => new BookingCreatedResponse(b.Id, b.TimeSlot.StartUtc, b.TimeSlot.EndUtc, b.Status))],
            skippedStarts);
    }

    private async Task SendConfirmationAsync(
        Booking firstBooking, ClientContact clientContact, int occurrenceCount, string organizationSlug, CancellationToken cancellationToken)
    {
        var confirmationLink = linkBuilder.Build($"/book/{organizationSlug}/confirm/{firstBooking.Id}?token={firstBooking.ConfirmationToken.Value}");
        var body = $"Confirm your {occurrenceCount}-visit series: {confirmationLink}";

        if (clientContact.Email is not null)
            await emailService.SendAsync(new EmailMessage(firstBooking.OrganizationId, clientContact.Email.Value, "Confirm your bookings", body), cancellationToken);

        await smsService.SendAsync(new SmsMessage(firstBooking.OrganizationId, clientContact.Phone.Value, body), cancellationToken);
    }
}