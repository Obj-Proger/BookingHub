using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Services.Commands.DeleteLocationServiceOverride;

internal sealed class DeleteLocationServiceOverrideCommandHandler(
    ILocationServiceOverrideRepository overrideRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteLocationServiceOverrideCommand>
{
    public async Task<Result> Handle(DeleteLocationServiceOverrideCommand command, CancellationToken cancellationToken)
    {
        var @override = await overrideRepository.GetByIdAsync(command.LocationId, command.OverrideId, cancellationToken);
        if (@override is null)
            return Result.Failure(ApplicationErrors.LocationServiceOverride.NotFound);

        overrideRepository.Remove(@override);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}