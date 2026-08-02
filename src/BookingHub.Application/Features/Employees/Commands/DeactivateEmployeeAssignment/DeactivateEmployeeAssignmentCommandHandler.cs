using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Employees.Commands.DeactivateEmployeeAssignment;

internal sealed class DeactivateEmployeeAssignmentCommandHandler(
    IEmployeeLocationAssignmentRepository assignmentRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivateEmployeeAssignmentCommand>
{
    public async Task<Result> Handle(DeactivateEmployeeAssignmentCommand command, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.GetByIdAsync(command.LocationId, command.AssignmentId, cancellationToken);
        if (assignment is null)
            return Result.Failure(ApplicationErrors.EmployeeLocationAssignment.NotFound);

        assignment.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}