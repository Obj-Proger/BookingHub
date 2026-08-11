using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Employees.DTOs;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Features.Employees.Commands.CreateRecurringSchedule;

internal sealed class CreateRecurringScheduleCommandHandler(
    IEmployeeLocationAssignmentRepository assignmentRepository,
    IRecurringScheduleRepository recurringScheduleRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRecurringScheduleCommand, RecurringScheduleCreatedResponse>
{
    public async Task<Result<RecurringScheduleCreatedResponse>> Handle(CreateRecurringScheduleCommand command, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.GetByIdAsync(command.LocationId, command.AssignmentId, cancellationToken);
        if (assignment is null)
            return Result.Failure<RecurringScheduleCreatedResponse>(ApplicationErrors.EmployeeLocationAssignment.NotFound);

        var siblingSchedules = await recurringScheduleRepository.GetByAssignmentAndDayAsync(assignment.Id, command.DayOfWeek, cancellationToken);
        if (siblingSchedules.Any(s => RecurringScheduleOverlap.Overlaps(command.StartTime, command.EndTime, s.StartTime, s.EndTime)))
            return Result.Failure<RecurringScheduleCreatedResponse>(ApplicationErrors.RecurringSchedule.Overlaps);

        var scheduleResult = RecurringSchedule.Create(assignment.Id, command.DayOfWeek, command.StartTime, command.EndTime);
        if (scheduleResult.IsFailure)
            return Result.Failure<RecurringScheduleCreatedResponse>(scheduleResult.Error);

        recurringScheduleRepository.Add(scheduleResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RecurringScheduleCreatedResponse(scheduleResult.Value.Id);
    }
}