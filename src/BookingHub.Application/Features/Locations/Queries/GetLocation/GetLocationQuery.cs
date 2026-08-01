using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Locations.DTOs;

namespace BookingHub.Application.Features.Locations.Queries.GetLocation;

public sealed record GetLocationQuery(Guid OrganizationId, Guid LocationId)
    : IQuery<LocationResponse>, IRequireOrganizationMembership;