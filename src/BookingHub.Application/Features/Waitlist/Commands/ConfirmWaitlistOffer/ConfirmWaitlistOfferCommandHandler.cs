using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings;
using BookingHub.Application.Features.Bookings.DTOs;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Waitlist.Commands.ConfirmWaitlistOffer;

internal sealed class ConfirmWaitlistOfferCommandHandler(
    IWaitlistEntryRepository waitlistEntryRepository,
    SlotAvailabilityService slotAvailability,
    IClientRepository clientRepository,
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<ConfirmWaitlistOfferCommand, BookingCreatedResponse>
{
    public async Task<Result<BookingCreatedResponse>> Handle(ConfirmWaitlistOfferCommand command, CancellationToken cancellationToken)
    {
        var entry = await waitlistEntryRepository.GetByIdAsync(command.WaitlistEntryId, cancellationToken);
        if (entry is null)
            return Result.Failure<BookingCreatedResponse>(ApplicationErrors.WaitlistEntry.NotFound);

        var providedToken = SecurityToken.FromExisting(command.Token ?? string.Empty);
        if (!entry.ManagementToken.Matches(providedToken))
            return Result.Failure<BookingCreatedResponse>(ApplicationErrors.WaitlistEntry.InvalidManagementToken);

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        // Status and expiry first: both are known without touching the schedule.
        var canConvert = entry.CanConvert(utcNow);
        if (canConvert.IsFailure)
            return Result.Failure<BookingCreatedResponse>(canConvert.Error);

        var offeredEmployeeId = entry.OfferedEmployeeId!.Value;

        // The same check as for any new booking: the slot must still fit the employee's schedule
        // and must not collide with a booking confirmed since the offer was made.
        var slotResult = await slotAvailability.CheckSlotAsync(
            entry.OrganizationId, entry.LocationId, offeredEmployeeId, entry.ServiceId, entry.OfferedSlot!.StartUtc,
            excludeBookingId: null, cancellationToken);
        if (slotResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(ApplicationErrors.Booking.SlotNotAvailable);

        var bookingResult = Booking.CreatePending(
            entry.OrganizationId, entry.LocationId, offeredEmployeeId, entry.ServiceId,
            entry.ClientContact, slotResult.Value.Slot, slotResult.Value.Price, BookingSource.Waitlist, utcNow);
        if (bookingResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(bookingResult.Error);

        var booking = bookingResult.Value;

        var convertResult = entry.Convert(utcNow);
        if (convertResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(convertResult.Error);

        // The guest already proved contact ownership by receiving and acting on the offer link —
        // a second, separate SMS/email confirmation for the resulting booking would be redundant.
        var confirmResult = booking.Confirm(utcNow);
        if (confirmResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(confirmResult.Error);

        var client = await clientRepository.GetByPhoneAsync(entry.ClientContact.Phone, cancellationToken);
        if (client is null)
        {
            client = Client.Create(entry.ClientContact);
            clientRepository.Add(client);
        }
        else
        {
            client.UpdateContactInfo(entry.ClientContact);
        }

        booking.LinkClient(client.Id);
        bookingRepository.Add(booking);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Failure<BookingCreatedResponse>(ApplicationErrors.Booking.SlotNotAvailable);
        }

        return new BookingCreatedResponse(booking.Id, booking.TimeSlot.StartUtc, booking.TimeSlot.EndUtc, booking.Status);
    }
}