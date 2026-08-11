using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Employees.DTOs;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Features.Employees.Commands.CreateModifiedHoursScheduleException;

internal sealed class CreateModifiedHoursScheduleExceptionCommandHandler(
    IEmployeeLocationAssignmentRepository assignmentRepository,
    IScheduleExceptionRepository scheduleExceptionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateModifiedHoursScheduleExceptionCommand, ScheduleExceptionCreatedResponse>
{
    public async Task<Result<ScheduleExceptionCreatedResponse>> Handle(CreateModifiedHoursScheduleExceptionCommand command, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.GetByIdAsync(command.LocationId, command.AssignmentId, cancellationToken);
        if (assignment is null)
            return Result.Failure<ScheduleExceptionCreatedResponse>(ApplicationErrors.EmployeeLocationAssignment.NotFound);

        if (await scheduleExceptionRepository.ExistsForDateAsync(assignment.Id, command.Date, cancellationToken))
            return Result.Failure<ScheduleExceptionCreatedResponse>(ApplicationErrors.ScheduleException.AlreadyExists);

        var exceptionResult = ScheduleException.CreateModifiedHours(
            assignment.Id, command.Date, command.ModifiedStartTime, command.ModifiedEndTime);
        if (exceptionResult.IsFailure)
            return Result.Failure<ScheduleExceptionCreatedResponse>(exceptionResult.Error);

        scheduleExceptionRepository.Add(exceptionResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ScheduleExceptionCreatedResponse(exceptionResult.Value.Id);
    }
}