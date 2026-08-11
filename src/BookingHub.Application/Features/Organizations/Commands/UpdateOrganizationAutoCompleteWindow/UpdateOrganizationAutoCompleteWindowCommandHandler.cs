using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Organizations.Commands.UpdateOrganizationAutoCompleteWindow;

internal sealed class UpdateOrganizationAutoCompleteWindowCommandHandler(
    IOrganizationRepository organizationRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateOrganizationAutoCompleteWindowCommand>
{
    public async Task<Result> Handle(UpdateOrganizationAutoCompleteWindowCommand command, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(command.OrganizationId, cancellationToken);
        if (organization is null)
            return Result.Failure(ApplicationErrors.Organization.NotFound);

        var updateResult = organization.UpdateAutoCompleteWindow(command.Window);
        if (updateResult.IsFailure)
            return updateResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}