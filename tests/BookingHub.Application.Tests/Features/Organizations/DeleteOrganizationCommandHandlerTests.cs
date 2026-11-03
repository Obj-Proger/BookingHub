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

    private static readonly Guid OrganizationId = Guid.CreateVersion7();
    private static readonly Guid UserId = Guid.CreateVersion7();

    private DeleteOrganizationCommandHandler CreateSut() =>
        new(_organizationRepository, _organizationMemberRepository, _currentUser);

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
        await _organizationRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OrganizationNotFound_ReturnsNotFound()
    {
        _currentUser.UserId.Returns(UserId);
        _organizationMemberRepository.GetByOrganizationAndUserAsync(OrganizationId, UserId, Arg.Any<CancellationToken>())
            .Returns(OrganizationMember.Create(OrganizationId, UserId, OrganizationRole.Owner, null, null).Value);
        _organizationRepository.GetSlugAsync(OrganizationId, Arg.Any<CancellationToken>()).Returns((string?)null);
        var sut = CreateSut();

        var result = await sut.Handle(new DeleteOrganizationCommand(OrganizationId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Organization.NotFound);
        await _organizationRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CallerIsOwner_DeletesTheOrganization()
    {
        _currentUser.UserId.Returns(UserId);
        _organizationMemberRepository.GetByOrganizationAndUserAsync(OrganizationId, UserId, Arg.Any<CancellationToken>())
            .Returns(OrganizationMember.Create(OrganizationId, UserId, OrganizationRole.Owner, null, null).Value);
        _organizationRepository.GetSlugAsync(OrganizationId, Arg.Any<CancellationToken>()).Returns("acme");
        var sut = CreateSut();

        var result = await sut.Handle(new DeleteOrganizationCommand(OrganizationId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _organizationRepository.Received(1).DeleteAsync(OrganizationId, Arg.Any<CancellationToken>());
    }
}