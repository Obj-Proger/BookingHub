using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Employees.DTOs;

namespace BookingHub.Application.Features.Employees.Commands.CreateDayOffScheduleException;

public sealed record CreateDayOffScheduleExceptionCommand(Guid OrganizationId, Guid LocationId, Guid AssignmentId, DateOnly Date)
    : ICommand<ScheduleExceptionCreatedResponse>, IRequireLocationManagement;