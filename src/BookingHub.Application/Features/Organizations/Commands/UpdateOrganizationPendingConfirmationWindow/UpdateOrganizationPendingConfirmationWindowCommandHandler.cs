using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Organizations.Commands.UpdateOrganizationPendingConfirmationWindow;

internal sealed class UpdateOrganizationPendingConfirmationWindowCommandHandler(
    IOrganizationRepository organizationRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateOrganizationPendingConfirmationWindowCommand>
{
    public async Task<Result> Handle(UpdateOrganizationPendingConfirmationWindowCommand command, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(command.OrganizationId, cancellationToken);
        if (organization is null)
            return Result.Failure(ApplicationErrors.Organization.NotFound);

        var updateResult = organization.UpdatePendingConfirmationWindow(command.Window);
        if (updateResult.IsFailure)
            return updateResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}