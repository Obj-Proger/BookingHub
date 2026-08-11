using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Employees.DTOs;

namespace BookingHub.Application.Features.Employees.Commands.CreateRecurringSchedule;

public sealed record CreateRecurringScheduleCommand(
    Guid OrganizationId, Guid LocationId, Guid AssignmentId, DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime)
    : ICommand<RecurringScheduleCreatedResponse>, IRequireLocationManagement;