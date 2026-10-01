using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Features.Public.DTOs;

namespace BookingHub.Application.Features.Public.Queries.GetPublicEmployees;

/// <summary>Anonymous by design — step 3 of the public booking flow (pick a bookable employee at the chosen location).</summary>
public sealed record GetPublicEmployeesQuery(Guid OrganizationId, Guid LocationId) : IQuery<IReadOnlyList<PublicEmployeeResponse>>;