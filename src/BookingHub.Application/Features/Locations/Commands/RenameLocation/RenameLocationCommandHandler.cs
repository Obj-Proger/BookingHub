using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Locations.Commands.RenameLocation;

internal sealed class RenameLocationCommandHandler(ILocationRepository locationRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<RenameLocationCommand>
{
    public async Task<Result> Handle(RenameLocationCommand command, CancellationToken cancellationToken)
    {
        var location = await locationRepository.GetByIdAsync(command.OrganizationId, command.LocationId, cancellationToken);
        if (location is null)
            return Result.Failure(ApplicationErrors.Location.NotFound);

        var renameResult = location.Rename(command.NewName);
        if (renameResult.IsFailure)
            return renameResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}