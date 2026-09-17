using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Organizations.DTOs;

namespace BookingHub.Application.Features.Organizations.Queries.GetMyOrganizationMemberships;

/// <summary>
/// No IRequireOrganizationMembership/organizationId at all — deliberately scoped only by the
/// caller's own UserId. Every other query in the project assumes the caller already knows which
/// organization to ask about; this is the one query that answers "which ones, and as what role"
/// in the first place, needed by any client right after login.
/// </summary>
public sealed record GetMyOrganizationMembershipsQuery : IQuery<IReadOnlyList<MyOrganizationMembershipResponse>>;