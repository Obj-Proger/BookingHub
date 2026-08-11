using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Organizations.Commands.UpdateOrganizationCancellationDeadline;

internal sealed class UpdateOrganizationCancellationDeadlineCommandHandler(
    IOrganizationRepository organizationRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateOrganizationCancellationDeadlineCommand>
{
    public async Task<Result> Handle(UpdateOrganizationCancellationDeadlineCommand command, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(command.OrganizationId, cancellationToken);
        if (organization is null)
            return Result.Failure(ApplicationErrors.Organization.NotFound);

        var updateResult = organization.UpdateCancellationDeadline(command.Hours);
        if (updateResult.IsFailure)
            return updateResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}