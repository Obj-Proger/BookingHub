using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Organizations.Commands.RemoveOrganizationMember;
using BookingHub.Application.Tests.TestDoubles;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Tests.Features.Organizations;

public class RemoveOrganizationMemberCommandHandlerTests
{
    private readonly IOrganizationMemberRepository _organizationMemberRepository = Substitute.For<IOrganizationMemberRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid CallerUserId = Guid.CreateVersion7();

    private RemoveOrganizationMemberCommandHandler CreateSut() =>
        new(_organizationMemberRepository, new FakeCurrentUser(CallerUserId), _unitOfWork);

    [Fact]
    public async Task Handle_RemovingNonOwnerMember_Succeeds()
    {
        var member = OrganizationMember.Create(OrganizationId, Guid.CreateVersion7(), OrganizationRole.Employee, employeeId: Guid.CreateVersion7()).Value;
        _organizationMemberRepository.GetByIdAsync(OrganizationId, member.Id, Arg.Any<CancellationToken>()).Returns(member);
        var sut = CreateSut();

        var result = await sut.Handle(new RemoveOrganizationMemberCommand(OrganizationId, member.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _organizationMemberRepository.Received(1).Remove(member);
    }

    [Fact]
    public async Task Handle_RemovingLastOwner_FailsWithCannotRemoveLastOwnerError()
    {
        var member = OrganizationMember.Create(OrganizationId, CallerUserId, OrganizationRole.Owner).Value;
        _organizationMemberRepository.GetByIdAsync(OrganizationId, member.Id, Arg.Any<CancellationToken>()).Returns(member);
        _organizationMemberRepository.GetByOrganizationAndUserAsync(OrganizationId, CallerUserId, Arg.Any<CancellationToken>()).Returns(member);
        _organizationMemberRepository.AnyOtherOwnerExistsAsync(OrganizationId, member.Id, Arg.Any<CancellationToken>()).Returns(false);
        var sut = CreateSut();

        var result = await sut.Handle(new RemoveOrganizationMemberCommand(OrganizationId, member.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.OrganizationMember.CannotRemoveLastOwner);
        _organizationMemberRepository.DidNotReceive().Remove(Arg.Any<OrganizationMember>());
    }

    [Fact]
    public async Task Handle_RemovingOwnerCallerIsNotOwner_FailsWithOnlyOwnerCanManageOwnerRoleError()
    {
        var member = OrganizationMember.Create(OrganizationId, Guid.CreateVersion7(), OrganizationRole.Owner).Value;
        var caller = OrganizationMember.Create(OrganizationId, CallerUserId, OrganizationRole.Administrator).Value;
        _organizationMemberRepository.GetByIdAsync(OrganizationId, member.Id, Arg.Any<CancellationToken>()).Returns(member);
        _organizationMemberRepository.GetByOrganizationAndUserAsync(OrganizationId, CallerUserId, Arg.Any<CancellationToken>()).Returns(caller);
        var sut = CreateSut();

        var result = await sut.Handle(new RemoveOrganizationMemberCommand(OrganizationId, member.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.OrganizationMember.OnlyOwnerCanManageOwnerRole);
    }
}