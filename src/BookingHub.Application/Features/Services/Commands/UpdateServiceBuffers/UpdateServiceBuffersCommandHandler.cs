using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Services.Commands.UpdateServiceBuffers;

internal sealed class UpdateServiceBuffersCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateServiceBuffersCommand>
{
    public async Task<Result> Handle(UpdateServiceBuffersCommand command, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(command.OrganizationId, command.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure(ApplicationErrors.Service.NotFound);

        var updateResult = service.UpdateBuffers(command.NewBufferBefore, command.NewBufferAfter);
        if (updateResult.IsFailure)
            return updateResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}