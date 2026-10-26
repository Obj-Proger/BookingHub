using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Employees.DTOs;

namespace BookingHub.Application.Features.Employees.Queries.GetEmployeeAssignments;

/// <summary>The locations an employee currently works at (active assignments only).</summary>
public sealed record GetEmployeeAssignmentsQuery(Guid OrganizationId, Guid EmployeeId)
    : IQuery<IReadOnlyList<EmployeeAssignmentResponse>>, IRequireOrganizationMembership;