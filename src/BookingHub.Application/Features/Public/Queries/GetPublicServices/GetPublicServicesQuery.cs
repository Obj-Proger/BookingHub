using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Public.DTOs;

namespace BookingHub.Application.Features.Public.Queries.GetPublicServices;

/// <summary>Anonymous by design — step 2 of the public booking flow (pick a service at the chosen location).</summary>
public sealed record GetPublicServicesQuery(Guid OrganizationId, Guid LocationId) : IQuery<IReadOnlyList<PublicServiceResponse>>;