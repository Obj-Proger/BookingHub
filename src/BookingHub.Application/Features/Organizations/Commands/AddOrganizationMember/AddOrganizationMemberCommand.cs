using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Organizations.DTOs;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Features.Organizations.Commands.AddOrganizationMember;

public sealed record AddOrganizationMemberCommand(
    Guid OrganizationId, Guid UserId, OrganizationRole Role, Guid? LocationId, Guid? EmployeeId)
    : ICommand<OrganizationMemberCreatedResponse>, IRequireOrganizationManagement;