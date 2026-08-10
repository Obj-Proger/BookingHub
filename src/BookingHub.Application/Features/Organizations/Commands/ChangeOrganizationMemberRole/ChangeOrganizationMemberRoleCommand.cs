using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Organizations.Commands.ChangeOrganizationMemberRole;

public sealed record ChangeOrganizationMemberRoleCommand(
    Guid OrganizationId, Guid OrganizationMemberId, OrganizationRole NewRole, Guid? LocationId, Guid? EmployeeId)
    : ICommand, IRequireOrganizationManagement;