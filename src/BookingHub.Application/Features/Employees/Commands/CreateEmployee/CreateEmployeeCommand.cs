using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Employees.DTOs;

namespace BookingHub.Application.Features.Employees.Commands.CreateEmployee;

public sealed record CreateEmployeeCommand(Guid OrganizationId, string? FullName)
    : ICommand<EmployeeCreatedResponse>, IRequireOrganizationManagement;