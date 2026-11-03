using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Organizations.Commands.DeleteOrganization;

internal sealed class DeleteOrganizationCommandHandler(
    IOrganizationRepository organizationRepository,
    IOrganizationMemberRepository organizationMemberRepository,
    ICurrentUser currentUser)
    : ICommandHandler<DeleteOrganizationCommand>
{
    public async Task<Result> Handle(DeleteOrganizationCommand command, CancellationToken cancellationToken)
    {
        var caller = await organizationMemberRepository.GetByOrganizationAndUserAsync(command.OrganizationId, currentUser.UserId, cancellationToken);
        if (caller is null || caller.Role != OrganizationRole.Owner)
            return Result.Failure(ApplicationErrors.Organization.OnlyOwnerCanDelete);

        if (await organizationRepository.GetSlugAsync(command.OrganizationId, cancellationToken) is null)
            return Result.Failure(ApplicationErrors.Organization.NotFound);

        await organizationRepository.DeleteAsync(command.OrganizationId, cancellationToken);
        return Result.Success();
    }
}