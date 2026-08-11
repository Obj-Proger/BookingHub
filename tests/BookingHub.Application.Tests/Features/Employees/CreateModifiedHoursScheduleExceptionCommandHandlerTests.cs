using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Employees.Commands.CreateModifiedHoursScheduleException;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Tests.Features.Employees;

public class CreateModifiedHoursScheduleExceptionCommandHandlerTests
{
    private readonly IEmployeeLocationAssignmentRepository _assignmentRepository = Substitute.For<IEmployeeLocationAssignmentRepository>();
    private readonly IScheduleExceptionRepository _scheduleExceptionRepository = Substitute.For<IScheduleExceptionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid LocationId = Guid.CreateVersion7();
    private static readonly Guid AssignmentId = Guid.CreateVersion7();
    private static readonly DateOnly Date = new(2026, 12, 25);

    private CreateModifiedHoursScheduleExceptionCommandHandler CreateSut() =>
        new(_assignmentRepository, _scheduleExceptionRepository, _unitOfWork);

    private static EmployeeLocationAssignment ValidAssignment() =>
        EmployeeLocationAssignment.Create(Guid.CreateVersion7(), Guid.CreateVersion7()).Value;

    [Fact]
    public async Task Handle_NoExistingExceptionForDate_Succeeds()
    {
        var assignment = ValidAssignment();
        _assignmentRepository.GetByIdAsync(LocationId, AssignmentId, Arg.Any<CancellationToken>()).Returns(assignment);
        _scheduleExceptionRepository.ExistsForDateAsync(assignment.Id, Date, Arg.Any<CancellationToken>()).Returns(false);
        var sut = CreateSut();

        var result = await sut.Handle(
            new CreateModifiedHoursScheduleExceptionCommand(OrganizationId, LocationId, AssignmentId, Date, new TimeOnly(10, 0), new TimeOnly(14, 0)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ExceptionAlreadyExistsForDate_FailsWithAlreadyExistsError()
    {
        var assignment = ValidAssignment();
        _assignmentRepository.GetByIdAsync(LocationId, AssignmentId, Arg.Any<CancellationToken>()).Returns(assignment);
        _scheduleExceptionRepository.ExistsForDateAsync(assignment.Id, Date, Arg.Any<CancellationToken>()).Returns(true);
        var sut = CreateSut();

        var result = await sut.Handle(
            new CreateModifiedHoursScheduleExceptionCommand(OrganizationId, LocationId, AssignmentId, Date, new TimeOnly(10, 0), new TimeOnly(14, 0)),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.ScheduleException.AlreadyExists);
        _scheduleExceptionRepository.DidNotReceive().Add(Arg.Any<ScheduleException>());
    }
}