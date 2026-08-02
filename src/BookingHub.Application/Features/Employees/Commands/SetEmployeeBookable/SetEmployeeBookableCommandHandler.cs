using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Employees.Commands.SetEmployeeBookable;

internal sealed class SetEmployeeBookableCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<SetEmployeeBookableCommand>
{
    public async Task<Result> Handle(SetEmployeeBookableCommand command, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(command.OrganizationId, command.EmployeeId, cancellationToken);
        if (employee is null)
            return Result.Failure(ApplicationErrors.Employee.NotFound);

        employee.SetBookable(command.IsBookable);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}