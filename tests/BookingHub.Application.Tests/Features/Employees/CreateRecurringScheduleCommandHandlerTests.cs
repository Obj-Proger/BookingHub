using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Employees.Commands.CreateRecurringSchedule;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Tests.Features.Employees;

public class CreateRecurringScheduleCommandHandlerTests
{
    private readonly IEmployeeLocationAssignmentRepository _assignmentRepository = Substitute.For<IEmployeeLocationAssignmentRepository>();
    private readonly IRecurringScheduleRepository _recurringScheduleRepository = Substitute.For<IRecurringScheduleRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid LocationId = Guid.CreateVersion7();
    private static readonly Guid AssignmentId = Guid.CreateVersion7();

    private CreateRecurringScheduleCommandHandler CreateSut() => new(_assignmentRepository, _recurringScheduleRepository, _unitOfWork);

    private static EmployeeLocationAssignment ValidAssignment() =>
        EmployeeLocationAssignment.Create(Guid.CreateVersion7(), Guid.CreateVersion7()).Value;

    [Fact]
    public async Task Handle_NoOverlappingSiblings_Succeeds()
    {
        var assignment = ValidAssignment();
        _assignmentRepository.GetByIdAsync(LocationId, AssignmentId, Arg.Any<CancellationToken>()).Returns(assignment);
        _recurringScheduleRepository.GetByAssignmentAndDayAsync(assignment.Id, DayOfWeek.Monday, Arg.Any<CancellationToken>())
            .Returns([]);
        var sut = CreateSut();

        var result = await sut.Handle(
            new CreateRecurringScheduleCommand(OrganizationId, LocationId, AssignmentId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(13, 0)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _recurringScheduleRepository.Received(1).Add(Arg.Any<RecurringSchedule>());
    }

    [Fact]
    public async Task Handle_NonOverlappingSplitShift_Succeeds()
    {
        var assignment = ValidAssignment();
        var morningShift = RecurringSchedule.Create(assignment.Id, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(13, 0)).Value;
        _assignmentRepository.GetByIdAsync(LocationId, AssignmentId, Arg.Any<CancellationToken>()).Returns(assignment);
        _recurringScheduleRepository.GetByAssignmentAndDayAsync(assignment.Id, DayOfWeek.Monday, Arg.Any<CancellationToken>())
            .Returns([morningShift]);
        var sut = CreateSut();

        var result = await sut.Handle(
            new CreateRecurringScheduleCommand(OrganizationId, LocationId, AssignmentId, DayOfWeek.Monday, new TimeOnly(15, 0), new TimeOnly(19, 0)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_OverlappingSibling_FailsWithOverlapsError()
    {
        var assignment = ValidAssignment();
        var existingShift = RecurringSchedule.Create(assignment.Id, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(13, 0)).Value;
        _assignmentRepository.GetByIdAsync(LocationId, AssignmentId, Arg.Any<CancellationToken>()).Returns(assignment);
        _recurringScheduleRepository.GetByAssignmentAndDayAsync(assignment.Id, DayOfWeek.Monday, Arg.Any<CancellationToken>())
            .Returns([existingShift]);
        var sut = CreateSut();

        var result = await sut.Handle(
            new CreateRecurringScheduleCommand(OrganizationId, LocationId, AssignmentId, DayOfWeek.Monday, new TimeOnly(12, 0), new TimeOnly(16, 0)),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.RecurringSchedule.Overlaps);
        _recurringScheduleRepository.DidNotReceive().Add(Arg.Any<RecurringSchedule>());
    }

    [Fact]
    public async Task Handle_AssignmentNotFoundForThisLocation_FailsWithoutCheckingOverlap()
    {
        _assignmentRepository.GetByIdAsync(LocationId, AssignmentId, Arg.Any<CancellationToken>()).Returns((EmployeeLocationAssignment?)null);
        var sut = CreateSut();

        var result = await sut.Handle(
            new CreateRecurringScheduleCommand(OrganizationId, LocationId, AssignmentId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(13, 0)),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.EmployeeLocationAssignment.NotFound);
        await _recurringScheduleRepository.DidNotReceive().GetByAssignmentAndDayAsync(Arg.Any<Guid>(), Arg.Any<DayOfWeek>(), Arg.Any<CancellationToken>());
    }
}