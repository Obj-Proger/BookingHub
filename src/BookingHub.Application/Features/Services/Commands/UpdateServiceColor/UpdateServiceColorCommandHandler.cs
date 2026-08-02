using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Services.Commands.UpdateServiceColor;

internal sealed class UpdateServiceColorCommandHandler(IServiceRepository serviceRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateServiceColorCommand>
{
    public async Task<Result> Handle(UpdateServiceColorCommand command, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(command.OrganizationId, command.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure(ApplicationErrors.Service.NotFound);

        var updateResult = service.UpdateColor(command.NewColor);
        if (updateResult.IsFailure)
            return updateResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}