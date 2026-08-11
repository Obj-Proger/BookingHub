using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Employees.Commands.RescheduleRecurringSchedule;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Tests.Features.Employees;

public class RescheduleRecurringScheduleCommandHandlerTests
{
    private readonly IRecurringScheduleRepository _recurringScheduleRepository = Substitute.For<IRecurringScheduleRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid LocationId = Guid.CreateVersion7();
    private static readonly Guid AssignmentId = Guid.CreateVersion7();

    private RescheduleRecurringScheduleCommandHandler CreateSut() => new(_recurringScheduleRepository, _unitOfWork);

    [Fact]
    public async Task Handle_NarrowingOwnTimeRangeSlightly_SucceedsDespiteBeingItsOwnSibling()
    {
        var schedule = RecurringSchedule.Create(AssignmentId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(13, 0)).Value;
        _recurringScheduleRepository.GetByIdAsync(LocationId, schedule.Id, Arg.Any<CancellationToken>()).Returns(schedule);
        _recurringScheduleRepository.GetByAssignmentAndDayAsync(AssignmentId, DayOfWeek.Monday, Arg.Any<CancellationToken>())
            .Returns([schedule]);
        var sut = CreateSut();

        var result = await sut.Handle(
            new RescheduleRecurringScheduleCommand(OrganizationId, LocationId, schedule.Id, new TimeOnly(9, 30), new TimeOnly(12, 30)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        schedule.StartTime.Should().Be(new TimeOnly(9, 30));
    }

    [Fact]
    public async Task Handle_NewTimeOverlapsADifferentSibling_FailsWithOverlapsError()
    {
        var schedule = RecurringSchedule.Create(AssignmentId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(13, 0)).Value;
        var afternoonShift = RecurringSchedule.Create(AssignmentId, DayOfWeek.Monday, new TimeOnly(15, 0), new TimeOnly(19, 0)).Value;
        _recurringScheduleRepository.GetByIdAsync(LocationId, schedule.Id, Arg.Any<CancellationToken>()).Returns(schedule);
        _recurringScheduleRepository.GetByAssignmentAndDayAsync(AssignmentId, DayOfWeek.Monday, Arg.Any<CancellationToken>())
            .Returns([schedule, afternoonShift]);
        var sut = CreateSut();

        var result = await sut.Handle(
            new RescheduleRecurringScheduleCommand(OrganizationId, LocationId, schedule.Id, new TimeOnly(9, 0), new TimeOnly(16, 0)),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.RecurringSchedule.Overlaps);
    }
}