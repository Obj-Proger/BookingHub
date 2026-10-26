using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Employees.DTOs;

namespace BookingHub.Application.Features.Employees.Queries.GetAssignmentSchedule;

public sealed record GetAssignmentScheduleQuery(Guid OrganizationId, Guid LocationId, Guid AssignmentId)
    : IQuery<AssignmentScheduleResponse>, IRequireOrganizationMembership;