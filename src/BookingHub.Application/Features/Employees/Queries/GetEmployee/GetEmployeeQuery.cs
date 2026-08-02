using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Employees.DTOs;

namespace BookingHub.Application.Features.Employees.Queries.GetEmployee;

public sealed record GetEmployeeQuery(Guid OrganizationId, Guid EmployeeId)
    : IQuery<EmployeeResponse>, IRequireOrganizationMembership;