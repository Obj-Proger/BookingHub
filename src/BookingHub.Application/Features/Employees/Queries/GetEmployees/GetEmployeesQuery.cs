using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Employees.DTOs;

namespace BookingHub.Application.Features.Employees.Queries.GetEmployees;

public sealed record GetEmployeesQuery(Guid OrganizationId)
    : IQuery<IReadOnlyList<EmployeeResponse>>, IRequireOrganizationMembership;