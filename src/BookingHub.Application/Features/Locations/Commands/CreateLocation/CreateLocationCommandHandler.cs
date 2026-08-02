using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Locations.DTOs;
using BookingHub.Domain.Entities;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Features.Locations.Commands.CreateLocation;

internal sealed class CreateLocationCommandHandler(ILocationRepository locationRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateLocationCommand, LocationCreatedResponse>
{
    public async Task<Result<LocationCreatedResponse>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var addressResult = Address.Create(command.Address);
        if (addressResult.IsFailure)
            return Result.Failure<LocationCreatedResponse>(addressResult.Error);

        var weeklyHoursResult = WeeklyHoursMapper.ToDomain(command.WorkingHours);
        if (weeklyHoursResult.IsFailure)
            return Result.Failure<LocationCreatedResponse>(weeklyHoursResult.Error);

        var locationResult = Location.Create(
            command.OrganizationId, command.Name, addressResult.Value, command.TimeZone, weeklyHoursResult.Value);
        if (locationResult.IsFailure)
            return Result.Failure<LocationCreatedResponse>(locationResult.Error);

        locationRepository.Add(locationResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LocationCreatedResponse(locationResult.Value.Id, locationResult.Value.Name);
    }
}