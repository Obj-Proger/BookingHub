using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Public.DTOs;

namespace BookingHub.Application.Features.Public.Queries.GetPublicLocations;

/// <summary>Anonymous by design — step 1 of the public booking flow (pick a location).</summary>
public sealed record GetPublicLocationsQuery(Guid OrganizationId) : IQuery<IReadOnlyList<PublicLocationResponse>>;