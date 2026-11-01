using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Notifications;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings.DTOs;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Bookings.Commands.CreateBooking;

internal sealed class CreateBookingCommandHandler(
    SlotAvailabilityService slotAvailability,
    IClientRepository clientRepository,
    IBookingRepository bookingRepository,
    IGuestMessages guestMessages,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateBookingCommand, BookingCreatedResponse>
{
    public async Task<Result<BookingCreatedResponse>> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        var contactResult = ClientContact.Create(command.Phone, command.ClientName, command.ClientEmail);
        if (contactResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(contactResult.Error);

        var clientContact = contactResult.Value;

        var slotResult = await slotAvailability.CheckSlotAsync(
            command.OrganizationId, command.LocationId, command.EmployeeId, command.ServiceId, command.StartUtc,
            excludeBookingId: null, cancellationToken);
        if (slotResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(slotResult.Error);

        var bookingResult = Booking.CreatePending(
            command.OrganizationId, command.LocationId, command.EmployeeId, command.ServiceId, clientContact,
            slotResult.Value.Slot, slotResult.Value.Price, BookingSource.Public, timeProvider.GetUtcNow().UtcDateTime);
        if (bookingResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(bookingResult.Error);

        var booking = bookingResult.Value;

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

        booking.LinkClient(client.Id);
        bookingRepository.Add(booking);
        await guestMessages.BookingRequestedAsync(booking, visitCount: 1, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new BookingCreatedResponse(booking.Id, booking.TimeSlot.StartUtc, booking.TimeSlot.EndUtc, booking.Status);
    }
}