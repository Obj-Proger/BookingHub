using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Waitlist.DTOs;
using BookingHub.Domain.Entities;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Waitlist.Commands.JoinWaitlist;

internal sealed class JoinWaitlistCommandHandler(
    ILocationRepository locationRepository,
    IServiceRepository serviceRepository,
    IEmployeeRepository employeeRepository,
    IWaitlistEntryRepository waitlistEntryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<JoinWaitlistCommand, WaitlistEntryCreatedResponse>
{
    public async Task<Result<WaitlistEntryCreatedResponse>> Handle(JoinWaitlistCommand command, CancellationToken cancellationToken)
    {
        var location = await locationRepository.GetByIdAsync(command.OrganizationId, command.LocationId, cancellationToken);
        if (location is null)
            return Result.Failure<WaitlistEntryCreatedResponse>(ApplicationErrors.Location.NotFound);

        var service = await serviceRepository.GetByIdAsync(command.OrganizationId, command.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure<WaitlistEntryCreatedResponse>(ApplicationErrors.Service.NotFound);

        if (command.EmployeeId is not null)
        {
            var employee = await employeeRepository.GetByIdAsync(command.OrganizationId, command.EmployeeId.Value, cancellationToken);
            if (employee is null)
                return Result.Failure<WaitlistEntryCreatedResponse>(ApplicationErrors.WaitlistEntry.EmployeeNotFound);
        }

        var phoneResult = PhoneNumber.Create(command.Phone);
        if (phoneResult.IsFailure)
            return Result.Failure<WaitlistEntryCreatedResponse>(phoneResult.Error);

        Email? email = null;
        if (!string.IsNullOrWhiteSpace(command.ClientEmail))
        {
            var emailResult = Email.Create(command.ClientEmail);
            if (emailResult.IsFailure)
                return Result.Failure<WaitlistEntryCreatedResponse>(emailResult.Error);
            email = emailResult.Value;
        }

        var windowResult = TimeSlot.Create(command.DesiredStartUtc, command.DesiredEndUtc);
        if (windowResult.IsFailure)
            return Result.Failure<WaitlistEntryCreatedResponse>(windowResult.Error);

        var clientContact = ClientContact.Create(phoneResult.Value, command.ClientName, email);

        var entryResult = WaitlistEntry.Create(
            command.OrganizationId, command.LocationId, command.EmployeeId, command.ServiceId,
            clientContact, windowResult.Value, DateTime.UtcNow);
        if (entryResult.IsFailure)
            return Result.Failure<WaitlistEntryCreatedResponse>(entryResult.Error);

        waitlistEntryRepository.Add(entryResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new WaitlistEntryCreatedResponse(entryResult.Value.Id);
    }
}