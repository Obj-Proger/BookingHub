using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Clients.DTOs;

namespace BookingHub.Application.Features.Clients.Queries.GetClientProfile;

public sealed record GetClientProfileQuery(Guid OrganizationId, Guid ClientId)
    : IQuery<ClientProfileResponse>, IRequireOrganizationMembership;