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
    SlotAvailabilityService slotAvailability,
    IClientRepository clientRepository,
    IBookingRepository bookingRepository,
    IGuestMessages guestMessages,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateRecurringBookingSeriesCommand, RecurringBookingSeriesCreatedResponse>
{
    public async Task<Result<RecurringBookingSeriesCreatedResponse>> Handle(
        CreateRecurringBookingSeriesCommand command, CancellationToken cancellationToken)
    {
        if (command.FirstStartUtc.Kind != DateTimeKind.Utc)
            return Result.Failure<RecurringBookingSeriesCreatedResponse>(DomainErrors.TimeSlot.NotUtc);

        var contactResult = ClientContact.Create(command.Phone, command.ClientName, command.ClientEmail);
        if (contactResult.IsFailure)
            return Result.Failure<RecurringBookingSeriesCreatedResponse>(contactResult.Error);

        var clientContact = contactResult.Value;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var client = await clientRepository.GetByPhoneAsync(clientContact.Phone, cancellationToken);
        if (await BookingRequestLimit.IsReachedAsync(bookingRepository, command.OrganizationId, client, utcNow, cancellationToken))
            return Result.Failure<RecurringBookingSeriesCreatedResponse>(ApplicationErrors.Booking.TooManyRequests);

        var seriesId = Guid.CreateVersion7();

        var createdBookings = new List<Booking>();
        var skippedStarts = new List<DateTime>();

        for (var occurrence = 0; occurrence < command.OccurrenceCount; occurrence++)
        {
            var occurrenceStartUtc = command.FirstStartUtc.AddDays(7 * command.IntervalWeeks * occurrence);

            // Best effort: an occurrence that cannot be booked is skipped and reported, not fatal.
            var slotResult = await slotAvailability.CheckSlotAsync(
                command.OrganizationId, command.LocationId, command.EmployeeId, command.ServiceId, occurrenceStartUtc,
                excludeBookingId: null, cancellationToken);

            var bookingResult = slotResult.IsSuccess
                ? Booking.CreatePending(
                    command.OrganizationId, command.LocationId, command.EmployeeId, command.ServiceId, clientContact,
                    slotResult.Value.Slot, slotResult.Value.Price, BookingSource.Public, utcNow, seriesId)
                : Result.Failure<Booking>(slotResult.Error);

            if (bookingResult.IsSuccess)
                createdBookings.Add(bookingResult.Value);
            else
                skippedStarts.Add(occurrenceStartUtc);
        }

        if (createdBookings.Count == 0)
            return Result.Failure<RecurringBookingSeriesCreatedResponse>(ApplicationErrors.Booking.NoOccurrenceAvailable);

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

        // One message for the whole series: confirming the first visit confirms the rest.
        await guestMessages.BookingRequestedAsync(createdBookings[0], createdBookings.Count, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RecurringBookingSeriesCreatedResponse(
            seriesId,
            [.. createdBookings.Select(b => new BookingCreatedResponse(b.Id, b.TimeSlot.StartUtc, b.TimeSlot.EndUtc, b.Status))],
            skippedStarts);
    }
}