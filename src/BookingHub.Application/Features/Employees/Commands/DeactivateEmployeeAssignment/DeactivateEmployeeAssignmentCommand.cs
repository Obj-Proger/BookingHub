using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Employees.Commands.DeactivateEmployeeAssignment;

public sealed record DeactivateEmployeeAssignmentCommand(Guid OrganizationId, Guid LocationId, Guid AssignmentId)
    : ICommand, IRequireLocationManagement;