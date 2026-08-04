using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Notifications;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Bookings.DTOs;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;
using BookingHub.Domain.Services;
using BookingHub.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

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

        var locationTimeZoneId = await dbContext.Locations
            .Where(l => l.Id == command.LocationId && l.OrganizationId == command.OrganizationId)
            .Select(l => l.TimeZone)
            .FirstOrDefaultAsync(cancellationToken);
        if (locationTimeZoneId is null)
            return Result.Failure<BookingCreatedResponse>(ApplicationErrors.Location.NotFound);

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(locationTimeZoneId);
        var localDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(command.StartUtc, timeZone));

        var contextResult = await AvailabilityContextLoader.LoadAsync(
            dbContext, command.OrganizationId, command.LocationId, command.EmployeeId, command.ServiceId, localDate, cancellationToken);
        if (contextResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(contextResult.Error);

        var context = contextResult.Value;
        if (context.Assignment is null)
            return Result.Failure<BookingCreatedResponse>(ApplicationErrors.Employee.NotAssignedToLocation);

        var availableSlots = AvailabilityCalculator.CalculateAvailableSlots(
            context.Location.WorkingHours, context.RecurringSchedule, context.ExceptionForDate, context.OccupiedWindows,
            context.Service.Duration, context.Service.BufferBefore, context.Service.BufferAfter,
            localDate, context.TimeZone, SlotGranularity);

        if (availableSlots.All(s => s.StartUtc != command.StartUtc))
            return Result.Failure<BookingCreatedResponse>(ApplicationErrors.Booking.SlotNotAvailable);

        var timeSlotResult = TimeSlot.Create(command.StartUtc, command.StartUtc + context.Service.Duration);
        if (timeSlotResult.IsFailure)
            return Result.Failure<BookingCreatedResponse>(timeSlotResult.Error);

        var clientContact = ClientContact.Create(phoneResult.Value, command.ClientName, email);

        var bookingResult = Booking.CreatePending(
            command.OrganizationId, command.LocationId, command.EmployeeId, command.ServiceId,
            clientContact, timeSlotResult.Value, BookingSource.Public, DateTime.UtcNow);
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