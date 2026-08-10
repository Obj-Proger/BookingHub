using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Organizations.Commands.RemoveOrganizationMember;

public sealed record RemoveOrganizationMemberCommand(Guid OrganizationId, Guid OrganizationMemberId) : ICommand, IRequireOrganizationManagement;