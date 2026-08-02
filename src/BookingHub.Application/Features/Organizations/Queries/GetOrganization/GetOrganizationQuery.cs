using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Organizations.DTOs;

namespace BookingHub.Application.Features.Organizations.Queries.GetOrganization;

public sealed record GetOrganizationQuery(Guid OrganizationId)
    : IQuery<OrganizationResponse>, IRequireOrganizationMembership;