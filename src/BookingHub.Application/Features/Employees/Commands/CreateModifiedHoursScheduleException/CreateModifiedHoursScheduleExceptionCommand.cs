using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Employees.DTOs;

namespace BookingHub.Application.Features.Employees.Commands.CreateModifiedHoursScheduleException;

public sealed record CreateModifiedHoursScheduleExceptionCommand(
    Guid OrganizationId, Guid LocationId, Guid AssignmentId, DateOnly Date, TimeOnly ModifiedStartTime, TimeOnly ModifiedEndTime)
    : ICommand<ScheduleExceptionCreatedResponse>, IRequireLocationManagement;