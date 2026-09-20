using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Employees.DTOs;

namespace BookingHub.Application.Features.Employees.Queries.GetMyEmployeeLocations;

/// <summary>Anonymous of EmployeeId by design — derived from the caller's own membership inside
/// the handler, never accepted as a parameter, so one employee can never query another's locations.</summary>
public sealed record GetMyEmployeeLocationsQuery(Guid OrganizationId)
    : IQuery<IReadOnlyList<EmployeeLocationResponse>>, IRequireOrganizationMembership;