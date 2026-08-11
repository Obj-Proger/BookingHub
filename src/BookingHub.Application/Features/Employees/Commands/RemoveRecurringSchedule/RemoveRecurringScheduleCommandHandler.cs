using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Employees.Commands.RemoveRecurringSchedule;

internal sealed class RemoveRecurringScheduleCommandHandler(
    IRecurringScheduleRepository recurringScheduleRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveRecurringScheduleCommand>
{
    public async Task<Result> Handle(RemoveRecurringScheduleCommand command, CancellationToken cancellationToken)
    {
        var schedule = await recurringScheduleRepository.GetByIdAsync(command.LocationId, command.RecurringScheduleId, cancellationToken);
        if (schedule is null)
            return Result.Failure(ApplicationErrors.RecurringSchedule.NotFound);

        recurringScheduleRepository.Remove(schedule);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}