using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Organizations.Commands.ChangeOrganizationMemberRole;
using BookingHub.Application.Tests.TestDoubles;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Tests.Features.Organizations;

public class ChangeOrganizationMemberRoleCommandHandlerTests
{
    private readonly IOrganizationMemberRepository _organizationMemberRepository = Substitute.For<IOrganizationMemberRepository>();
    private readonly ILocationRepository _locationRepository = Substitute.For<ILocationRepository>();
    private readonly IEmployeeRepository _employeeRepository = Substitute.For<IEmployeeRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid CallerUserId = Guid.CreateVersion7();

    private ChangeOrganizationMemberRoleCommandHandler CreateSut() =>
        new(_organizationMemberRepository, _locationRepository, _employeeRepository, new FakeCurrentUser(CallerUserId), _unitOfWork);

    [Fact]
    public async Task Handle_DemotingOwnerWhenAnotherOwnerExists_Succeeds()
    {
        var member = OrganizationMember.Create(OrganizationId, Guid.CreateVersion7(), OrganizationRole.Owner).Value;
        var caller = OrganizationMember.Create(OrganizationId, CallerUserId, OrganizationRole.Owner).Value;
        _organizationMemberRepository.GetByIdAsync(OrganizationId, member.Id, Arg.Any<CancellationToken>()).Returns(member);
        _organizationMemberRepository.GetByOrganizationAndUserAsync(OrganizationId, CallerUserId, Arg.Any<CancellationToken>()).Returns(caller);
        _organizationMemberRepository.AnyOtherOwnerExistsAsync(OrganizationId, member.Id, Arg.Any<CancellationToken>()).Returns(true);
        var sut = CreateSut();

        var result = await sut.Handle(
            new ChangeOrganizationMemberRoleCommand(OrganizationId, member.Id, OrganizationRole.Administrator, null, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        member.Role.Should().Be(OrganizationRole.Administrator);
    }

    [Fact]
    public async Task Handle_DemotingLastOwner_FailsWithCannotRemoveLastOwnerError()
    {
        var member = OrganizationMember.Create(OrganizationId, CallerUserId, OrganizationRole.Owner).Value;
        _organizationMemberRepository.GetByIdAsync(OrganizationId, member.Id, Arg.Any<CancellationToken>()).Returns(member);
        _organizationMemberRepository.GetByOrganizationAndUserAsync(OrganizationId, CallerUserId, Arg.Any<CancellationToken>()).Returns(member);
        _organizationMemberRepository.AnyOtherOwnerExistsAsync(OrganizationId, member.Id, Arg.Any<CancellationToken>()).Returns(false);
        var sut = CreateSut();

        var result = await sut.Handle(
            new ChangeOrganizationMemberRoleCommand(OrganizationId, member.Id, OrganizationRole.Administrator, null, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.OrganizationMember.CannotRemoveLastOwner);
        member.Role.Should().Be(OrganizationRole.Owner);
    }

    [Fact]
    public async Task Handle_PromotingToOwnerCallerIsNotOwner_FailsWithOnlyOwnerCanManageOwnerRoleError()
    {
        var member = OrganizationMember.Create(OrganizationId, Guid.CreateVersion7(), OrganizationRole.Administrator).Value;
        var caller = OrganizationMember.Create(OrganizationId, CallerUserId, OrganizationRole.Administrator).Value;
        _organizationMemberRepository.GetByIdAsync(OrganizationId, member.Id, Arg.Any<CancellationToken>()).Returns(member);
        _organizationMemberRepository.GetByOrganizationAndUserAsync(OrganizationId, CallerUserId, Arg.Any<CancellationToken>()).Returns(caller);
        var sut = CreateSut();

        var result = await sut.Handle(
            new ChangeOrganizationMemberRoleCommand(OrganizationId, member.Id, OrganizationRole.Owner, null, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.OrganizationMember.OnlyOwnerCanManageOwnerRole);
    }

    [Fact]
    public async Task Handle_MemberNotFound_FailsWithNotFoundError()
    {
        _organizationMemberRepository.GetByIdAsync(OrganizationId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((OrganizationMember?)null);
        var sut = CreateSut();

        var result = await sut.Handle(
            new ChangeOrganizationMemberRoleCommand(OrganizationId, Guid.CreateVersion7(), OrganizationRole.Administrator, null, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.OrganizationMember.NotFound);
    }
}