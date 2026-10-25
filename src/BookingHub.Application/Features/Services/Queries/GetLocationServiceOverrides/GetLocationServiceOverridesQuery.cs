using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Services.DTOs;

namespace BookingHub.Application.Features.Services.Queries.GetLocationServiceOverrides;

public sealed record GetLocationServiceOverridesQuery(Guid OrganizationId, Guid LocationId)
    : IQuery<IReadOnlyList<LocationServiceOverrideResponse>>, IRequireOrganizationMembership;