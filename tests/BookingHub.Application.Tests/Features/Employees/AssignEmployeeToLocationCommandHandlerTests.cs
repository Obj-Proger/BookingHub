using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Employees.Commands.AssignEmployeeToLocation;
using BookingHub.Domain.Entities;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Tests.Features.Employees;

public class AssignEmployeeToLocationCommandHandlerTests
{
    private readonly ILocationRepository _locationRepository = Substitute.For<ILocationRepository>();
    private readonly IEmployeeRepository _employeeRepository = Substitute.For<IEmployeeRepository>();
    private readonly IEmployeeLocationAssignmentRepository _assignmentRepository = Substitute.For<IEmployeeLocationAssignmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid LocationId = Guid.CreateVersion7();
    private static readonly Guid EmployeeId = Guid.CreateVersion7();

    private AssignEmployeeToLocationCommandHandler CreateSut() =>
        new(_locationRepository, _employeeRepository, _assignmentRepository, _unitOfWork);

    private static Location ValidLocation() => Location.Create(
        OrganizationId, "Downtown", Address.Create("221B Baker Street").Value, "UTC",
        WeeklyHours.Create(Enum.GetValues<DayOfWeek>().Select(DailyHours.CreateClosed)).Value).Value;

    [Fact]
    public async Task Handle_LocationAndEmployeeBelongToOrganization_CreatesAssignment()
    {
        _locationRepository.GetByIdAsync(OrganizationId, LocationId, Arg.Any<CancellationToken>()).Returns(ValidLocation());
        _employeeRepository.GetByIdAsync(OrganizationId, EmployeeId, Arg.Any<CancellationToken>())
            .Returns(Employee.Create(OrganizationId, "Jane Doe").Value);
        var sut = CreateSut();

        var result = await sut.Handle(new AssignEmployeeToLocationCommand(OrganizationId, LocationId, EmployeeId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _assignmentRepository.Received(1).Add(Arg.Any<EmployeeLocationAssignment>());
    }

    [Fact]
    public async Task Handle_LocationNotFoundInOrganization_FailsWithoutLookingUpEmployee()
    {
        _locationRepository.GetByIdAsync(OrganizationId, LocationId, Arg.Any<CancellationToken>()).Returns((Location?)null);
        var sut = CreateSut();

        var result = await sut.Handle(new AssignEmployeeToLocationCommand(OrganizationId, LocationId, EmployeeId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Location.NotFound);
        await _employeeRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmployeeNotFoundInOrganization_FailsWithEmployeeNotFoundError()
    {
        _locationRepository.GetByIdAsync(OrganizationId, LocationId, Arg.Any<CancellationToken>()).Returns(ValidLocation());
        _employeeRepository.GetByIdAsync(OrganizationId, EmployeeId, Arg.Any<CancellationToken>()).Returns((Employee?)null);
        var sut = CreateSut();

        var result = await sut.Handle(new AssignEmployeeToLocationCommand(OrganizationId, LocationId, EmployeeId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Employee.NotFound);
    }
}