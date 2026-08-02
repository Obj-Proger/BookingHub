using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Services.Commands.RenameService;

internal sealed class RenameServiceCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<RenameServiceCommand>
{
    public async Task<Result> Handle(RenameServiceCommand command, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(command.OrganizationId, command.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure(ApplicationErrors.Service.NotFound);

        var renameResult = service.Rename(command.NewName);
        if (renameResult.IsFailure)
            return renameResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}