using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Organizations.Commands.UpdateOrganizationCancellationDeadline;
using BookingHub.Domain.Entities;

namespace BookingHub.Application.Tests.Features.Organizations;

public class UpdateOrganizationCancellationDeadlineCommandHandlerTests
{
    private readonly IOrganizationRepository _organizationRepository = Substitute.For<IOrganizationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private UpdateOrganizationCancellationDeadlineCommandHandler CreateSut() => new(_organizationRepository, _unitOfWork);

    [Fact]
    public async Task Handle_ValidHours_UpdatesDeadline()
    {
        var organization = Organization.Create("Name", "slug").Value;
        _organizationRepository.GetByIdAsync(organization.Id, Arg.Any<CancellationToken>()).Returns(organization);
        var sut = CreateSut();

        var result = await sut.Handle(new UpdateOrganizationCancellationDeadlineCommand(organization.Id, 48), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        organization.CancellationDeadlineHours.Should().Be(48);
    }

    [Fact]
    public async Task Handle_NegativeHours_FailsWithDomainCancellationDeadlineNegativeError()
    {
        var organization = Organization.Create("Name", "slug").Value;
        _organizationRepository.GetByIdAsync(organization.Id, Arg.Any<CancellationToken>()).Returns(organization);
        var sut = CreateSut();

        var result = await sut.Handle(new UpdateOrganizationCancellationDeadlineCommand(organization.Id, -1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Organization.CancellationDeadlineNegative);
        organization.CancellationDeadlineHours.Should().Be(24);
    }

    [Fact]
    public async Task Handle_OrganizationNotFound_FailsWithNotFoundError()
    {
        _organizationRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Organization?)null);
        var sut = CreateSut();

        var result = await sut.Handle(new UpdateOrganizationCancellationDeadlineCommand(Guid.CreateVersion7(), 48), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Organization.NotFound);
    }
}