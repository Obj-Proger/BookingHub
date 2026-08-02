using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Locations.Commands.UpdateLocationWorkingHours;

internal sealed class UpdateLocationWorkingHoursCommandHandler(ILocationRepository locationRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateLocationWorkingHoursCommand>
{
    public async Task<Result> Handle(UpdateLocationWorkingHoursCommand command, CancellationToken cancellationToken)
    {
        var location = await locationRepository.GetByIdAsync(command.OrganizationId, command.LocationId, cancellationToken);
        if (location is null)
            return Result.Failure(ApplicationErrors.Location.NotFound);

        var weeklyHoursResult = WeeklyHoursMapper.ToDomain(command.WorkingHours);
        if (weeklyHoursResult.IsFailure)
            return Result.Failure(weeklyHoursResult.Error);

        location.UpdateWorkingHours(weeklyHoursResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}