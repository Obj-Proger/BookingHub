using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Employees.DTOs;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Features.Employees.Commands.AssignEmployeeToLocation;

internal sealed class AssignEmployeeToLocationCommandHandler(
    ILocationRepository locationRepository,
    IEmployeeRepository employeeRepository,
    IEmployeeLocationAssignmentRepository assignmentRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignEmployeeToLocationCommand, EmployeeAssignmentCreatedResponse>
{
    public async Task<Result<EmployeeAssignmentCreatedResponse>> Handle(AssignEmployeeToLocationCommand command, CancellationToken cancellationToken)
    {
        var location = await locationRepository.GetByIdAsync(command.OrganizationId, command.LocationId, cancellationToken);
        if (location is null)
            return Result.Failure<EmployeeAssignmentCreatedResponse>(ApplicationErrors.Location.NotFound);

        var employee = await employeeRepository.GetByIdAsync(command.OrganizationId, command.EmployeeId, cancellationToken);
        if (employee is null)
            return Result.Failure<EmployeeAssignmentCreatedResponse>(ApplicationErrors.Employee.NotFound);

        var assignmentResult = EmployeeLocationAssignment.Create(employee.Id, location.Id);
        if (assignmentResult.IsFailure)
            return Result.Failure<EmployeeAssignmentCreatedResponse>(assignmentResult.Error);

        assignmentRepository.Add(assignmentResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new EmployeeAssignmentCreatedResponse(assignmentResult.Value.Id, employee.Id, location.Id);
    }
}