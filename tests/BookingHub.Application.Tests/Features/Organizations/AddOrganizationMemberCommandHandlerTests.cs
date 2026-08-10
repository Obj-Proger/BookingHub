using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Organizations.Commands.AddOrganizationMember;
using BookingHub.Application.Tests.TestDoubles;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Tests.Features.Organizations;

public class AddOrganizationMemberCommandHandlerTests
{
    private readonly IOrganizationMemberRepository _organizationMemberRepository = Substitute.For<IOrganizationMemberRepository>();
    private readonly ILocationRepository _locationRepository = Substitute.For<ILocationRepository>();
    private readonly IEmployeeRepository _employeeRepository = Substitute.For<IEmployeeRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid CallerUserId = Guid.CreateVersion7();
    private static readonly Guid TargetUserId = Guid.CreateVersion7();

    private AddOrganizationMemberCommandHandler CreateSut() =>
        new(_organizationMemberRepository, _locationRepository, _employeeRepository, new FakeCurrentUser(CallerUserId), _unitOfWork);

    [Fact]
    public async Task Handle_AdministratorRole_Succeeds()
    {
        _organizationMemberRepository.ExistsAsync(OrganizationId, TargetUserId, Arg.Any<CancellationToken>()).Returns(false);
        var sut = CreateSut();

        var result = await sut.Handle(
            new AddOrganizationMemberCommand(OrganizationId, TargetUserId, OrganizationRole.Administrator, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _organizationMemberRepository.Received(1).Add(Arg.Any<OrganizationMember>());
    }

    [Fact]
    public async Task Handle_OwnerRoleCallerIsNotOwner_FailsWithOnlyOwnerCanManageOwnerRoleError()
    {
        var caller = OrganizationMember.Create(OrganizationId, CallerUserId, OrganizationRole.Administrator).Value;
        _organizationMemberRepository.GetByOrganizationAndUserAsync(OrganizationId, CallerUserId, Arg.Any<CancellationToken>()).Returns(caller);
        var sut = CreateSut();

        var result = await sut.Handle(
            new AddOrganizationMemberCommand(OrganizationId, TargetUserId, OrganizationRole.Owner, null, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.OrganizationMember.OnlyOwnerCanManageOwnerRole);
        _organizationMemberRepository.DidNotReceive().Add(Arg.Any<OrganizationMember>());
    }

    [Fact]
    public async Task Handle_OwnerRoleCallerIsOwner_Succeeds()
    {
        var caller = OrganizationMember.Create(OrganizationId, CallerUserId, OrganizationRole.Owner).Value;
        _organizationMemberRepository.GetByOrganizationAndUserAsync(OrganizationId, CallerUserId, Arg.Any<CancellationToken>()).Returns(caller);
        _organizationMemberRepository.ExistsAsync(OrganizationId, TargetUserId, Arg.Any<CancellationToken>()).Returns(false);
        var sut = CreateSut();

        var result = await sut.Handle(
            new AddOrganizationMemberCommand(OrganizationId, TargetUserId, OrganizationRole.Owner, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UserAlreadyMember_FailsWithAlreadyMemberError()
    {
        _organizationMemberRepository.ExistsAsync(OrganizationId, TargetUserId, Arg.Any<CancellationToken>()).Returns(true);
        var sut = CreateSut();

        var result = await sut.Handle(
            new AddOrganizationMemberCommand(OrganizationId, TargetUserId, OrganizationRole.Administrator, null, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.OrganizationMember.AlreadyMember);
    }

    [Fact]
    public async Task Handle_LocationManagerRoleLocationNotFound_FailsWithLocationNotFoundError()
    {
        _organizationMemberRepository.ExistsAsync(OrganizationId, TargetUserId, Arg.Any<CancellationToken>()).Returns(false);
        _locationRepository.GetByIdAsync(OrganizationId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Location?)null);
        var sut = CreateSut();

        var result = await sut.Handle(
            new AddOrganizationMemberCommand(OrganizationId, TargetUserId, OrganizationRole.LocationManager, Guid.CreateVersion7(), null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Location.NotFound);
    }
}