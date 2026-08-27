using BookingHub.Domain.Entities;
using BookingHub.Application.Common.Persistence;
using BookingHub.Application.Features.Organizations.Commands.UpdateOrganizationCancellationDeadline;

namespace BookingHub.Application.Tests.Features.Organizations;

public class UpdateOrganizationCancellationDeadlineCommandHandlerTests
{
    private readonly IOrganizationRepository _organizationRepository = Substitute.For<IOrganizationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private UpdateOrganizationCancellationDeadlineCommandHandler CreateSut() => new(_organizationRepository, _unitOfWork);

    [Fact]
    public async Task Handle_ValidDeadline_UpdatesDeadline()
    {
        var organization = Organization.Create("Name", "slug").Value;
        _organizationRepository.GetByIdAsync(organization.Id, Arg.Any<CancellationToken>()).Returns(organization);
        var sut = CreateSut();

        var result = await sut.Handle(new UpdateOrganizationCancellationDeadlineCommand(organization.Id, TimeSpan.FromHours(48)), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        organization.CancellationDeadline.Should().Be(TimeSpan.FromHours(48));
    }

    [Fact]
    public async Task Handle_NegativeDeadline_FailsWithDomainCancellationDeadlineNegativeError()
    {
        var organization = Organization.Create("Name", "slug").Value;
        _organizationRepository.GetByIdAsync(organization.Id, Arg.Any<CancellationToken>()).Returns(organization);
        var sut = CreateSut();

        var result = await sut.Handle(new UpdateOrganizationCancellationDeadlineCommand(organization.Id, TimeSpan.FromHours(-1)), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Organization.CancellationDeadlineNegative);
        organization.CancellationDeadline.Should().Be(TimeSpan.FromHours(24));
    }

    [Fact]
    public async Task Handle_OrganizationNotFound_FailsWithNotFoundError()
    {
        _organizationRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Organization?)null);
        var sut = CreateSut();

        var result = await sut.Handle(new UpdateOrganizationCancellationDeadlineCommand(Guid.CreateVersion7(), TimeSpan.FromHours(48)), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.Organization.NotFound);
    }
}