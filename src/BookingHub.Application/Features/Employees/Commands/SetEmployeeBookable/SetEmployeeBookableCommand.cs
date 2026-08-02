using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Employees.Commands.SetEmployeeBookable;

public sealed record SetEmployeeBookableCommand(Guid OrganizationId, Guid EmployeeId, bool IsBookable)
    : ICommand, IRequireOrganizationManagement;