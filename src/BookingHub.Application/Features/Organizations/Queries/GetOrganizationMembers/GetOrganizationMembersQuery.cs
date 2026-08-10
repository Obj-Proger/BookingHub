using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Organizations.DTOs;

namespace BookingHub.Application.Features.Organizations.Queries.GetOrganizationMembers;

public sealed record GetOrganizationMembersQuery(Guid OrganizationId)
    : IQuery<IReadOnlyList<OrganizationMemberResponse>>, IRequireOrganizationManagement;