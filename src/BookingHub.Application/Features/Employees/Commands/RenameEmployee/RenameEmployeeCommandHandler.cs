using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Employees.Commands.RenameEmployee;

internal sealed class RenameEmployeeCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<RenameEmployeeCommand>
{
    public async Task<Result> Handle(RenameEmployeeCommand command, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(command.OrganizationId, command.EmployeeId, cancellationToken);
        if (employee is null)
            return Result.Failure(ApplicationErrors.Employee.NotFound);

        var renameResult = employee.Rename(command.NewFullName);
        if (renameResult.IsFailure)
            return renameResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}