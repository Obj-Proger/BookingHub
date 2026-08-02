using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Employees.DTOs;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Features.Employees.Commands.CreateEmployee;

internal sealed class CreateEmployeeCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateEmployeeCommand, EmployeeCreatedResponse>
{
    public async Task<Result<EmployeeCreatedResponse>> Handle(CreateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var employeeResult = Employee.Create(command.OrganizationId, command.FullName);
        if (employeeResult.IsFailure)
            return Result.Failure<EmployeeCreatedResponse>(employeeResult.Error);

        employeeRepository.Add(employeeResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new EmployeeCreatedResponse(employeeResult.Value.Id, employeeResult.Value.FullName);
    }
}