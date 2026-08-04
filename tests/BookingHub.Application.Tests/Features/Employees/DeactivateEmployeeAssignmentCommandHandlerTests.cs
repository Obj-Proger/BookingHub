using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Employees.Commands.DeactivateEmployeeAssignment;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Tests.Features.Employees;

public class DeactivateEmployeeAssignmentCommandHandlerTests
{
    private readonly IEmployeeLocationAssignmentRepository _assignmentRepository = Substitute.For<IEmployeeLocationAssignmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid LocationId = Guid.CreateVersion7();
    private static readonly Guid AssignmentId = Guid.CreateVersion7();

    private DeactivateEmployeeAssignmentCommandHandler CreateSut() => new(_assignmentRepository, _unitOfWork);

    [Fact]
    public async Task Handle_AssignmentFoundForThisLocation_Deactivates()
    {
        var assignment = EmployeeLocationAssignment.Create(Guid.CreateVersion7(), LocationId).Value;
        _assignmentRepository.GetByIdAsync(LocationId, AssignmentId, Arg.Any<CancellationToken>()).Returns(assignment);
        var sut = CreateSut();

        var result = await sut.Handle(new DeactivateEmployeeAssignmentCommand(OrganizationId, LocationId, AssignmentId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        assignment.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AssignmentNotFoundForThisLocation_FailsWithNotFoundError()
    {
        _assignmentRepository.GetByIdAsync(LocationId, AssignmentId, Arg.Any<CancellationToken>()).Returns((EmployeeLocationAssignment?)null);
        var sut = CreateSut();

        var result = await sut.Handle(new DeactivateEmployeeAssignmentCommand(OrganizationId, LocationId, AssignmentId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.EmployeeLocationAssignment.NotFound);
    }
}