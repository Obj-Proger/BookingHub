using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Organizations.Commands.SetOrganizationAdministratorFinancialAccess;
using BookingHub.Application.Tests.TestDoubles;
using BookingHub.Domain.Enums;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Tests.Features.Organizations;

public class SetOrganizationAdministratorFinancialAccessCommandHandlerTests
{
    private readonly IOrganizationRepository _organizationRepository = Substitute.For<IOrganizationRepository>();
    private readonly IOrganizationMemberRepository _organizationMemberRepository = Substitute.For<IOrganizationMemberRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid CallerUserId = Guid.CreateVersion7();

    private SetOrganizationAdministratorFinancialAccessCommandHandler CreateSut() =>
        new(_organizationRepository, _organizationMemberRepository, new FakeCurrentUser(CallerUserId), _unitOfWork);

    [Fact]
    public async Task Handle_CallerIsOwner_EnablesAccess()
    {
        var caller = OrganizationMember.Create(OrganizationId, CallerUserId, OrganizationRole.Owner).Value;
        var organization = Organization.Create("Name", "slug").Value;
        _organizationMemberRepository.GetByOrganizationAndUserAsync(OrganizationId, CallerUserId, Arg.Any<CancellationToken>()).Returns(caller);
        _organizationRepository.GetByIdAsync(OrganizationId, Arg.Any<CancellationToken>()).Returns(organization);
        var sut = CreateSut();

        var result = await sut.Handle(new SetOrganizationAdministratorFinancialAccessCommand(OrganizationId, true), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        organization.CanAdministratorsViewFinancials.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CallerIsAdministrator_FailsWithOnlyOwnerCanManageOwnerRoleError()
    {
        var caller = OrganizationMember.Create(OrganizationId, CallerUserId, OrganizationRole.Administrator).Value;
        _organizationMemberRepository.GetByOrganizationAndUserAsync(OrganizationId, CallerUserId, Arg.Any<CancellationToken>()).Returns(caller);
        var sut = CreateSut();

        var result = await sut.Handle(new SetOrganizationAdministratorFinancialAccessCommand(OrganizationId, true), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.OrganizationMember.OnlyOwnerCanManageOwnerRole);
        await _organizationRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}