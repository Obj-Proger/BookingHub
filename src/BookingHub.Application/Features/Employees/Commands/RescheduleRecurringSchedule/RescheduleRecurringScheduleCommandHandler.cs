using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Employees.Commands.RescheduleRecurringSchedule;

internal sealed class RescheduleRecurringScheduleCommandHandler(
    IRecurringScheduleRepository recurringScheduleRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<RescheduleRecurringScheduleCommand>
{
    public async Task<Result> Handle(RescheduleRecurringScheduleCommand command, CancellationToken cancellationToken)
    {
        var schedule = await recurringScheduleRepository.GetByIdAsync(command.LocationId, command.RecurringScheduleId, cancellationToken);
        if (schedule is null)
            return Result.Failure(ApplicationErrors.RecurringSchedule.NotFound);

        var siblingSchedules = await recurringScheduleRepository.GetByAssignmentAndDayAsync(
            schedule.EmployeeLocationAssignmentId, schedule.DayOfWeek, cancellationToken);

        var overlapsWithSibling = siblingSchedules.Any(s =>
            s.Id != schedule.Id && RecurringScheduleOverlap.Overlaps(command.NewStartTime, command.NewEndTime, s.StartTime, s.EndTime));
        if (overlapsWithSibling)
            return Result.Failure(ApplicationErrors.RecurringSchedule.Overlaps);

        var rescheduleResult = schedule.Reschedule(command.NewStartTime, command.NewEndTime);
        if (rescheduleResult.IsFailure)
            return rescheduleResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}