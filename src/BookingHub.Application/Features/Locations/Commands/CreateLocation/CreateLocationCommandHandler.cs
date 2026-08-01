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

        var dailyHours = new List<DailyHours>();
        foreach (var dto in command.WorkingHours)
        {
            if (dto.OpenTime is null || dto.CloseTime is null)
            {
                dailyHours.Add(DailyHours.CreateClosed(dto.DayOfWeek));
                continue;
            }

            var dailyResult = DailyHours.CreateOpen(dto.DayOfWeek, dto.OpenTime.Value, dto.CloseTime.Value);
            if (dailyResult.IsFailure)
                return Result.Failure<LocationCreatedResponse>(dailyResult.Error);

            dailyHours.Add(dailyResult.Value);
        }

        var weeklyHoursResult = WeeklyHours.Create(dailyHours);
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