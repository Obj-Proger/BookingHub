using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Locations.DTOs;

namespace BookingHub.Application.Features.Locations.Queries.GetLocations;

public sealed record GetLocationsQuery(Guid OrganizationId)
    : IQuery<IReadOnlyList<LocationResponse>>, IRequireOrganizationMembership;