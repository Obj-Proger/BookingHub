using BookingHub.Application.Common.Security;
using BookingHub.Application.Common.Messaging;

namespace BookingHub.Application.Features.Employees.Commands.RemoveRecurringSchedule;

public sealed record RemoveRecurringScheduleCommand(Guid OrganizationId, Guid LocationId, Guid RecurringScheduleId)
    : ICommand, IRequireLocationManagement;