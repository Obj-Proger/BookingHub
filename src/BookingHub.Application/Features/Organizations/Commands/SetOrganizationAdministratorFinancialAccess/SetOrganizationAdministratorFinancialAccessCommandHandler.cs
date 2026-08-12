using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Organizations.Commands.SetOrganizationAdministratorFinancialAccess;

internal sealed class SetOrganizationAdministratorFinancialAccessCommandHandler(
    IOrganizationRepository organizationRepository,
    IOrganizationMemberRepository organizationMemberRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SetOrganizationAdministratorFinancialAccessCommand>
{
    public async Task<Result> Handle(SetOrganizationAdministratorFinancialAccessCommand command, CancellationToken cancellationToken)
    {
        var caller = await organizationMemberRepository.GetByOrganizationAndUserAsync(command.OrganizationId, currentUser.UserId, cancellationToken);
        if (caller is null || caller.Role != OrganizationRole.Owner)
            return Result.Failure(ApplicationErrors.OrganizationMember.OnlyOwnerCanManageOwnerRole);

        var organization = await organizationRepository.GetByIdAsync(command.OrganizationId, cancellationToken);
        if (organization is null)
            return Result.Failure(ApplicationErrors.Organization.NotFound);

        organization.SetAdministratorFinancialAccess(command.Enabled);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}