using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Services.Commands.UpdateServiceDuration;

internal sealed class UpdateServiceDurationCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateServiceDurationCommand>
{
    public async Task<Result> Handle(UpdateServiceDurationCommand command, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(command.OrganizationId, command.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure(ApplicationErrors.Service.NotFound);

        var updateResult = service.UpdateDuration(command.NewDuration);
        if (updateResult.IsFailure)
            return updateResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}