using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Organizations.Commands.UpdateOrganizationWaitlistOfferWindow;

internal sealed class UpdateOrganizationWaitlistOfferWindowCommandHandler(
    IOrganizationRepository organizationRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateOrganizationWaitlistOfferWindowCommand>
{
    public async Task<Result> Handle(UpdateOrganizationWaitlistOfferWindowCommand command, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(command.OrganizationId, cancellationToken);
        if (organization is null)
            return Result.Failure(ApplicationErrors.Organization.NotFound);

        var updateResult = organization.UpdateWaitlistOfferWindow(command.Window);
        if (updateResult.IsFailure)
            return updateResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}