using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Services.Commands.CreateLocationServiceOverride;
using BookingHub.Domain.Entities;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Application.Tests.Features.Services;

public class CreateLocationServiceOverrideCommandHandlerTests
{
    private readonly ILocationRepository _locationRepository = Substitute.For<ILocationRepository>();
    private readonly IServiceRepository _serviceRepository = Substitute.For<IServiceRepository>();
    private readonly ILocationServiceOverrideRepository _overrideRepository = Substitute.For<ILocationServiceOverrideRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid LocationId = Guid.CreateVersion7();
    private static readonly Guid ServiceId = Guid.CreateVersion7();

    private CreateLocationServiceOverrideCommandHandler CreateSut() =>
        new(_locationRepository, _serviceRepository, _overrideRepository, _unitOfWork);

    private static Location ValidLocation() => Location.Create(
        OrganizationId, "Downtown", Address.Create("221B Baker Street").Value, "UTC",
        WeeklyHours.Create(Enum.GetValues<DayOfWeek>().Select(DailyHours.CreateClosed)).Value).Value;

    private static Service ValidService(string currency) => Service.Create(
        OrganizationId, "Haircut", TimeSpan.FromMinutes(30), Money.Create(50m, currency).Value,
        TimeSpan.Zero, TimeSpan.Zero, "#FF5733").Value;

    [Fact]
    public async Task Handle_OverrideCurrencyMatchesServiceCurrency_Succeeds()
    {
        _locationRepository.GetByIdAsync(OrganizationId, LocationId, Arg.Any<CancellationToken>()).Returns(ValidLocation());
        _serviceRepository.GetByIdAsync(OrganizationId, ServiceId, Arg.Any<CancellationToken>()).Returns(ValidService("USD"));
        _overrideRepository.ExistsForServiceAsync(LocationId, ServiceId, Arg.Any<CancellationToken>()).Returns(false);
        var sut = CreateSut();

        var result = await sut.Handle(
            new CreateLocationServiceOverrideCommand(OrganizationId, LocationId, ServiceId, 45m, "USD"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_OverrideCurrencyDiffersFromServiceCurrency_FailsWithoutCheckingExistence()
    {
        _locationRepository.GetByIdAsync(OrganizationId, LocationId, Arg.Any<CancellationToken>()).Returns(ValidLocation());
        _serviceRepository.GetByIdAsync(OrganizationId, ServiceId, Arg.Any<CancellationToken>()).Returns(ValidService("USD"));
        var sut = CreateSut();

        var result = await sut.Handle(
            new CreateLocationServiceOverrideCommand(OrganizationId, LocationId, ServiceId, 45m, "EUR"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.LocationServiceOverride.CurrencyMismatch);
        await _overrideRepository.DidNotReceive().ExistsForServiceAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OverrideAlreadyExistsForServiceAtLocation_FailsWithAlreadyExistsError()
    {
        _locationRepository.GetByIdAsync(OrganizationId, LocationId, Arg.Any<CancellationToken>()).Returns(ValidLocation());
        _serviceRepository.GetByIdAsync(OrganizationId, ServiceId, Arg.Any<CancellationToken>()).Returns(ValidService("USD"));
        _overrideRepository.ExistsForServiceAsync(LocationId, ServiceId, Arg.Any<CancellationToken>()).Returns(true);
        var sut = CreateSut();

        var result = await sut.Handle(
            new CreateLocationServiceOverrideCommand(OrganizationId, LocationId, ServiceId, 45m, "USD"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.LocationServiceOverride.AlreadyExists);
    }
}