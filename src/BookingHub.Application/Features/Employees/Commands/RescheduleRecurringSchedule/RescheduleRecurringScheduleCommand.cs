using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Security;

namespace BookingHub.Application.Features.Employees.Commands.RescheduleRecurringSchedule;

public sealed record RescheduleRecurringScheduleCommand(
    Guid OrganizationId, Guid LocationId, Guid RecurringScheduleId, TimeOnly NewStartTime, TimeOnly NewEndTime)
    : ICommand, IRequireLocationManagement;