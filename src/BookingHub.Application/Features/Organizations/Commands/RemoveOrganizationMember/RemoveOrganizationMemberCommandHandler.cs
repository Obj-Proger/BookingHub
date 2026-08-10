using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Organizations.Commands.RemoveOrganizationMember;

internal sealed class RemoveOrganizationMemberCommandHandler(
    IOrganizationMemberRepository organizationMemberRepository, ICurrentUser currentUser, IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveOrganizationMemberCommand>
{
    public async Task<Result> Handle(RemoveOrganizationMemberCommand command, CancellationToken cancellationToken)
    {
        var member = await organizationMemberRepository.GetByIdAsync(command.OrganizationId, command.OrganizationMemberId, cancellationToken);
        if (member is null)
            return Result.Failure(ApplicationErrors.OrganizationMember.NotFound);

        if (member.Role == OrganizationRole.Owner)
        {
            var caller = await organizationMemberRepository.GetByOrganizationAndUserAsync(command.OrganizationId, currentUser.UserId, cancellationToken);
            if (caller is null || caller.Role != OrganizationRole.Owner)
                return Result.Failure(ApplicationErrors.OrganizationMember.OnlyOwnerCanManageOwnerRole);

            if (!await organizationMemberRepository.AnyOtherOwnerExistsAsync(command.OrganizationId, member.Id, cancellationToken))
                return Result.Failure(ApplicationErrors.OrganizationMember.CannotRemoveLastOwner);
        }

        organizationMemberRepository.Remove(member);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}