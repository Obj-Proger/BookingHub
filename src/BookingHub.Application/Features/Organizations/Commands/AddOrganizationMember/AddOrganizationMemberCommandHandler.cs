using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Organizations.DTOs;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Organizations.Commands.AddOrganizationMember;

internal sealed class AddOrganizationMemberCommandHandler(
    IOrganizationMemberRepository organizationMemberRepository,
    ILocationRepository locationRepository,
    IEmployeeRepository employeeRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddOrganizationMemberCommand, OrganizationMemberCreatedResponse>
{
    public async Task<Result<OrganizationMemberCreatedResponse>> Handle(AddOrganizationMemberCommand command, CancellationToken cancellationToken)
    {
        if (command.Role == OrganizationRole.Owner)
        {
            var caller = await organizationMemberRepository.GetByOrganizationAndUserAsync(command.OrganizationId, currentUser.UserId, cancellationToken);
            if (caller is null || caller.Role != OrganizationRole.Owner)
                return Result.Failure<OrganizationMemberCreatedResponse>(ApplicationErrors.OrganizationMember.OnlyOwnerCanManageOwnerRole);
        }

        if (await organizationMemberRepository.ExistsAsync(command.OrganizationId, command.UserId, cancellationToken))
            return Result.Failure<OrganizationMemberCreatedResponse>(ApplicationErrors.OrganizationMember.AlreadyMember);

        if (command.Role == OrganizationRole.LocationManager)
        {
            var location = await locationRepository.GetByIdAsync(command.OrganizationId, command.LocationId ?? Guid.Empty, cancellationToken);
            if (location is null)
                return Result.Failure<OrganizationMemberCreatedResponse>(ApplicationErrors.Location.NotFound);
        }

        if (command.Role == OrganizationRole.Employee)
        {
            var employee = await employeeRepository.GetByIdAsync(command.OrganizationId, command.EmployeeId ?? Guid.Empty, cancellationToken);
            if (employee is null)
                return Result.Failure<OrganizationMemberCreatedResponse>(ApplicationErrors.Employee.NotFound);
        }

        var memberResult = OrganizationMember.Create(command.OrganizationId, command.UserId, command.Role, command.LocationId, command.EmployeeId);
        if (memberResult.IsFailure)
            return Result.Failure<OrganizationMemberCreatedResponse>(memberResult.Error);

        organizationMemberRepository.Add(memberResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new OrganizationMemberCreatedResponse(memberResult.Value.Id);
    }
}