using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Employees.DTOs;

namespace BookingHub.Application.Features.Employees.Commands.AssignEmployeeToLocation;

public sealed record AssignEmployeeToLocationCommand(Guid OrganizationId, Guid LocationId, Guid EmployeeId)
    : ICommand<EmployeeAssignmentCreatedResponse>, IRequireLocationManagement;