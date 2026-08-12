using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings;
using BookingHub.Application.Features.Bookings.DTOs;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.Services;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookingHub.Application.Features.Waitlist.Commands.ConfirmWaitlistOffer;

internal sealed class ConfirmWaitlistOfferCommandHandler(
    IWaitlistEntryRepository waitlistEntryRepository,
    IApplicationDbContext dbContext,
    IClientRepository clientRepository,
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork)
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

        if (entry.Status != WaitlistEntryStatus.Offered)
            return Result.Failure<BookingCreatedResponse>(DomainErrors.WaitlistEntry.CannotConvert);

        var offeredSlot = entry.OfferedSlot!;
        var offeredEmployeeId = entry.OfferedEmployeeId!.Value;

        var locationTimeZoneId = await dbContext.Locations
            .Where(l => l.Id == entry.LocationId)
            .Select(l => l.TimeZone)
            .FirstAsync(cancellationToken);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(locationTimeZoneId);
        var localDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(offeredSlot.StartUtc, timeZone));

        var contextResult = await AvailabilityContextLoader.LoadAsync(
            dbContext, entry.OrganizationId, entry.LocationId, offeredEmployeeId, entry.ServiceId, localDate, cancellationToken);
        if (contextResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(contextResult.Error);

        var context = contextResult.Value;

        if (!AvailabilityCalculator.IsSlotAvailable(offeredSlot, context.Service.BufferBefore, context.Service.BufferAfter, context.OccupiedWindows))
            return Result.Failure<BookingCreatedResponse>(ApplicationErrors.Booking.SlotNotAvailable);

        var convertResult = entry.Convert(DateTime.UtcNow);
        if (convertResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(convertResult.Error);

        var overrideEntity = await dbContext.LocationServiceOverrides
            .FirstOrDefaultAsync(o => o.LocationId == entry.LocationId && o.ServiceId == entry.ServiceId, cancellationToken);
        var effectivePrice = overrideEntity?.OverridePrice ?? context.Service.BasePrice;

        var bookingResult = Booking.CreatePending(
            entry.OrganizationId, entry.LocationId, offeredEmployeeId, entry.ServiceId,
            entry.ClientContact, offeredSlot, effectivePrice, BookingSource.Waitlist, DateTime.UtcNow);
        if (bookingResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(bookingResult.Error);

        var booking = bookingResult.Value;

        // The guest already proved contact ownership by receiving and acting on the offer link —
        // a second, separate SMS/email confirmation for the resulting booking would be redundant.
        var confirmResult = booking.Confirm(DateTime.UtcNow);
        if (confirmResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(confirmResult.Error);

        var client = await clientRepository.GetByPhoneAsync(entry.ClientContact.Phone, cancellationToken);
        if (client is null)
        {
            client = Client.Create(entry.ClientContact.Phone, entry.ClientContact.Name, entry.ClientContact.Email);
            clientRepository.Add(client);
        }
        else
        {
            client.UpdateContactInfo(entry.ClientContact.Name, entry.ClientContact.Email);
        }

        booking.LinkClient(client.Id);
        bookingRepository.Add(booking);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new BookingCreatedResponse(booking.Id, booking.TimeSlot.StartUtc, booking.TimeSlot.EndUtc, booking.Status);
    }
}