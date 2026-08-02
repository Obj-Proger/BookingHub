using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Employees.Commands.RenameEmployee;

public sealed record RenameEmployeeCommand(Guid OrganizationId, Guid EmployeeId, string? NewFullName)
    : ICommand, IRequireOrganizationManagement;