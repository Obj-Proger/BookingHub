using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Employees.DTOs;

namespace BookingHub.Application.Features.Employees.Queries.GetLocationAssignments;

/// <summary>The employees currently working at a location (active assignments only).</summary>
public sealed record GetLocationAssignmentsQuery(Guid OrganizationId, Guid LocationId)
    : IQuery<IReadOnlyList<LocationAssignmentResponse>>, IRequireOrganizationMembership;