using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Organizations.Commands.AddOrganizationMember;
using BookingHub.Application.Features.Organizations.DTOs;

namespace BookingHub.Application.Features.Organizations.Commands.AddOrganizationMemberByEmail;

/// <summary>Resolves the email to a user and hands over to <see cref="AddOrganizationMemberCommand"/>,
/// which owns every rule about roles, scopes and duplicates.</summary>
internal sealed class AddOrganizationMemberByEmailCommandHandler(IUserDirectory userDirectory, IDispatcher dispatcher)
    : ICommandHandler<AddOrganizationMemberByEmailCommand, OrganizationMemberCreatedResponse>
{
    public async Task<Result<OrganizationMemberCreatedResponse>> Handle(
        AddOrganizationMemberByEmailCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            return Result.Failure<OrganizationMemberCreatedResponse>(ApplicationErrors.OrganizationMember.UserNotFound);

        var userId = await userDirectory.FindUserIdByEmailAsync(command.Email.Trim(), cancellationToken);
        if (userId is null)
            return Result.Failure<OrganizationMemberCreatedResponse>(ApplicationErrors.OrganizationMember.UserNotFound);

        return await dispatcher.Send(
            new AddOrganizationMemberCommand(command.OrganizationId, userId.Value, command.Role, command.LocationId, command.EmployeeId),
            cancellationToken);
    }
}