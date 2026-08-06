using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Waitlist.Commands.JoinWaitlist;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Tests.Features.Waitlist;

public class JoinWaitlistCommandHandlerTests
{
    private readonly ILocationRepository _locationRepository = Substitute.For<ILocationRepository>();
    private readonly IServiceRepository _serviceRepository = Substitute.For<IServiceRepository>();
    private readonly IEmployeeRepository _employeeRepository = Substitute.For<IEmployeeRepository>();
    private readonly IWaitlistEntryRepository _waitlistEntryRepository = Substitute.For<IWaitlistEntryRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid LocationId = Guid.CreateVersion7();
    private static readonly Guid ServiceId = Guid.CreateVersion7();

    private JoinWaitlistCommandHandler CreateSut() =>
        new(_locationRepository, _serviceRepository, _employeeRepository, _waitlistEntryRepository, _unitOfWork);

    private JoinWaitlistCommand ValidCommand(Guid? employeeId = null) => new(
        OrganizationId, LocationId, employeeId, ServiceId,
        DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), "+14155552671", "Jane Doe", null);

    private static Location ValidLocation() => Location.Create(
        OrganizationId, "Downtown", Domain.ValueObjects.Address.Create("221B Baker Street").Value, "UTC",
        Domain.ValueObjects.WeeklyHours.Create(Enum.GetValues<DayOfWeek>().Select(Domain.ValueObjects.DailyHours.CreateClosed)).Value).Value;

    private static Service ValidService() => Service.Create(
        OrganizationId, "Haircut", TimeSpan.FromMinutes(30), Domain.ValueObjects.Money.Create(50m, "USD").Value,
        TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;

    [Fact]
    public async Task Handle_ValidCommandWithoutSpecificEmployee_JoinsWaitlist()
    {
        _locationRepository.GetByIdAsync(OrganizationId, LocationId, Arg.Any<CancellationToken>()).Returns(ValidLocation());
        _serviceRepository.GetByIdAsync(OrganizationId, ServiceId, Arg.Any<CancellationToken>()).Returns(ValidService());
        var sut = CreateSut();

        var result = await sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _waitlistEntryRepository.Received(1).Add(Arg.Any<Domain.Entities.WaitlistEntry>());
        await _employeeRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SpecificEmployeeNotFound_FailsWithEmployeeNotFoundError()
    {
        _locationRepository.GetByIdAsync(OrganizationId, LocationId, Arg.Any<CancellationToken>()).Returns(ValidLocation());
        _serviceRepository.GetByIdAsync(OrganizationId, ServiceId, Arg.Any<CancellationToken>()).Returns(ValidService());
        var employeeId = Guid.CreateVersion7();
        _employeeRepository.GetByIdAsync(OrganizationId, employeeId, Arg.Any<CancellationToken>()).Returns((Employee?)null);
        var sut = CreateSut();

        var result = await sut.Handle(ValidCommand(employeeId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.WaitlistEntry.EmployeeNotFound);
    }

    [Fact]
    public async Task Handle_LocationNotFound_FailsWithoutQueryingService()
    {
        _locationRepository.GetByIdAsync(OrganizationId, LocationId, Arg.Any<CancellationToken>()).Returns((Location?)null);
        var sut = CreateSut();

        var result = await sut.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Location.NotFound);
        await _serviceRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}