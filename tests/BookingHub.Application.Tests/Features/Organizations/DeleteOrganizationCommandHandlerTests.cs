using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Common.Security;
using BookingHub.Application.Features.Organizations.Commands.DeleteOrganization;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Tests.Features.Organizations;

public class DeleteOrganizationCommandHandlerTests
{
    private readonly IOrganizationRepository _organizationRepository = Substitute.For<IOrganizationRepository>();
    private readonly IOrganizationMemberRepository _organizationMemberRepository = Substitute.For<IOrganizationMemberRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid UserId = Guid.CreateVersion7();

    private DeleteOrganizationCommandHandler CreateSut() =>
        new(_organizationRepository, _organizationMemberRepository, _currentUser, _unitOfWork);

    [Fact]
    public async Task Handle_CallerNotOwner_ReturnsForbidden()
    {
        _currentUser.UserId.Returns(UserId);
        _organizationMemberRepository.GetByOrganizationAndUserAsync(OrganizationId, UserId, Arg.Any<CancellationToken>())
            .Returns(OrganizationMember.Create(OrganizationId, UserId, OrganizationRole.Administrator, null, null).Value);
        var sut = CreateSut();

        var result = await sut.Handle(new DeleteOrganizationCommand(OrganizationId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Organization.OnlyOwnerCanDelete);
        await _organizationRepository.DidNotReceive().DeleteAllDataAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OrganizationNotFound_ReturnsNotFound()
    {
        _currentUser.UserId.Returns(UserId);
        _organizationMemberRepository.GetByOrganizationAndUserAsync(OrganizationId, UserId, Arg.Any<CancellationToken>())
            .Returns(OrganizationMember.Create(OrganizationId, UserId, OrganizationRole.Owner, null, null).Value);
        _organizationRepository.GetByIdAsync(OrganizationId, Arg.Any<CancellationToken>()).Returns((Organization?)null);
        var sut = CreateSut();

        var result = await sut.Handle(new DeleteOrganizationCommand(OrganizationId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Organization.NotFound);
    }

    [Fact]
    public async Task Handle_CallerIsOwner_DeletesAllDataThenRemovesOrganization()
    {
        var organization = Organization.Create("Acme", "acme").Value;
        _currentUser.UserId.Returns(UserId);
        _organizationMemberRepository.GetByOrganizationAndUserAsync(OrganizationId, UserId, Arg.Any<CancellationToken>())
            .Returns(OrganizationMember.Create(OrganizationId, UserId, OrganizationRole.Owner, null, null).Value);
        _organizationRepository.GetByIdAsync(OrganizationId, Arg.Any<CancellationToken>()).Returns(organization);
        var sut = CreateSut();

        var result = await sut.Handle(new DeleteOrganizationCommand(OrganizationId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _organizationRepository.Received(1).DeleteAllDataAsync(OrganizationId, Arg.Any<CancellationToken>());
        _organizationRepository.Received(1).Remove(organization);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}