using BookingHub.Application.Common.Security;
using BookingHub.Application.Common.Messaging;

namespace BookingHub.Application.Features.Employees.Commands.RemoveScheduleException;
public sealed record RemoveScheduleExceptionCommand(Guid OrganizationId, Guid LocationId, Guid ScheduleExceptionId)
    : ICommand, IRequireLocationManagement;