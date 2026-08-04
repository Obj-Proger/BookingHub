using BookingHub.Application.Common;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Organizations.Commands.CreateOrganization;
using BookingHub.Application.Tests.TestDoubles;
using BookingHub.Domain.Entities;
using BookingHub.Domain.Enums;

namespace BookingHub.Application.Tests.Features.Organizations;

public class CreateOrganizationCommandHandlerTests
{
    private readonly IOrganizationRepository _organizationRepository = Substitute.For<IOrganizationRepository>();
    private readonly IOrganizationMemberRepository _organizationMemberRepository = Substitute.For<IOrganizationMemberRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private static readonly Guid CallerUserId = Guid.CreateVersion7();

    private CreateOrganizationCommandHandler CreateSut() =>
        new(_organizationRepository, _organizationMemberRepository, new FakeCurrentUser(CallerUserId), _unitOfWork);

    [Fact]
    public async Task Handle_ValidCommand_CreatesOrganizationAndOwnerMember()
    {
        _organizationRepository.SlugExistsAsync("bright-smile", Arg.Any<CancellationToken>()).Returns(false);
        var sut = CreateSut();

        var result = await sut.Handle(new CreateOrganizationCommand("Bright Smile", "bright-smile"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _organizationRepository.Received(1).Add(Arg.Is<Organization>(o => o.Slug == "bright-smile"));
        _organizationMemberRepository.Received(1).Add(
            Arg.Is<OrganizationMember>(m => m.Role == OrganizationRole.Owner && m.UserId == CallerUserId));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SlugAlreadyTaken_FailsWithoutSavingAnything()
    {
        _organizationRepository.SlugExistsAsync("taken", Arg.Any<CancellationToken>()).Returns(true);
        var sut = CreateSut();

        var result = await sut.Handle(new CreateOrganizationCommand("Name", "taken"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Organization.SlugAlreadyTaken);
        _organizationRepository.DidNotReceive().Add(Arg.Any<Organization>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidName_FailsWithoutQueryingSlugExistence()
    {
        var sut = CreateSut();

        var result = await sut.Handle(new CreateOrganizationCommand(null, "valid-slug"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Organization.NameEmpty);
        await _organizationRepository.DidNotReceive().SlugExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}