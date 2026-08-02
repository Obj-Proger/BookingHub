using BookingHub.Application.Common;
using BookingHub.Application.Common.Messaging;
using BookingHub.Application.Common.Persistence;

namespace BookingHub.Application.Features.Employees.Commands.UpdateEmployeePhoto;

internal sealed class UpdateEmployeePhotoCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateEmployeePhotoCommand>
{
    public async Task<Result> Handle(UpdateEmployeePhotoCommand command, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(command.OrganizationId, command.EmployeeId, cancellationToken);
        if (employee is null)
            return Result.Failure(ApplicationErrors.Employee.NotFound);

        var updateResult = employee.UpdatePhoto(command.PhotoUrl);
        if (updateResult.IsFailure)
            return updateResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}