using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Organizations.Commands.ChangeOrganizationMemberRole;

internal sealed class ChangeOrganizationMemberRoleCommandHandler(
    IOrganizationMemberRepository organizationMemberRepository,
    ILocationRepository locationRepository,
    IEmployeeRepository employeeRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeOrganizationMemberRoleCommand>
{
    public async Task<Result> Handle(ChangeOrganizationMemberRoleCommand command, CancellationToken cancellationToken)
    {
        var member = await organizationMemberRepository.GetByIdAsync(command.OrganizationId, command.OrganizationMemberId, cancellationToken);
        if (member is null)
            return Result.Failure(ApplicationErrors.OrganizationMember.NotFound);

        var touchesOwnerRole = member.Role == OrganizationRole.Owner || command.NewRole == OrganizationRole.Owner;
        if (touchesOwnerRole)
        {
            var caller = await organizationMemberRepository.GetByOrganizationAndUserAsync(command.OrganizationId, currentUser.UserId, cancellationToken);
            if (caller is null || caller.Role != OrganizationRole.Owner)
                return Result.Failure(ApplicationErrors.OrganizationMember.OnlyOwnerCanManageOwnerRole);
        }

        if (member.Role == OrganizationRole.Owner && command.NewRole != OrganizationRole.Owner
            && !await organizationMemberRepository.AnyOtherOwnerExistsAsync(command.OrganizationId, member.Id, cancellationToken))
        {
            return Result.Failure(ApplicationErrors.OrganizationMember.CannotRemoveLastOwner);
        }

        if (command.NewRole == OrganizationRole.LocationManager)
        {
            var location = await locationRepository.GetByIdAsync(command.OrganizationId, command.LocationId ?? Guid.Empty, cancellationToken);
            if (location is null)
                return Result.Failure(ApplicationErrors.Location.NotFound);
        }

        if (command.NewRole == OrganizationRole.Employee)
        {
            var employee = await employeeRepository.GetByIdAsync(command.OrganizationId, command.EmployeeId ?? Guid.Empty, cancellationToken);
            if (employee is null)
                return Result.Failure(ApplicationErrors.Employee.NotFound);
        }

        var changeResult = member.ChangeRole(command.NewRole, command.LocationId, command.EmployeeId);
        if (changeResult.IsFailure)
            return changeResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}