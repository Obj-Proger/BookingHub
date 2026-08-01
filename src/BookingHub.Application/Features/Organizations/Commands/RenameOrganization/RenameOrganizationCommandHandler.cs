using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Organizations.Commands.RenameOrganization;

internal sealed class RenameOrganizationCommandHandler(IOrganizationRepository organizationRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<RenameOrganizationCommand>
{
    public async Task<Result> Handle(RenameOrganizationCommand command, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(command.OrganizationId, cancellationToken);
        if (organization is null)
            return Result.Failure(ApplicationErrors.Organization.NotFound);

        var renameResult = organization.Rename(command.NewName);
        if (renameResult.IsFailure)
            return renameResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}