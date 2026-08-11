using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Employees.Commands.RemoveScheduleException;

internal sealed class RemoveScheduleExceptionCommandHandler(
    IScheduleExceptionRepository scheduleExceptionRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveScheduleExceptionCommand>
{
    public async Task<Result> Handle(RemoveScheduleExceptionCommand command, CancellationToken cancellationToken)
    {
        var exception = await scheduleExceptionRepository.GetByIdAsync(command.LocationId, command.ScheduleExceptionId, cancellationToken);
        if (exception is null)
            return Result.Failure(ApplicationErrors.ScheduleException.NotFound);

        scheduleExceptionRepository.Remove(exception);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}