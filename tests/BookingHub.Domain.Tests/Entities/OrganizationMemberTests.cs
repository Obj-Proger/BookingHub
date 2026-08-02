namespace BookingHub.Domain.Tests.Entities;

public class OrganizationMemberTests
{
    private static readonly Guid ValidOrganizationId = Guid.CreateVersion7();
    private static readonly Guid ValidUserId = Guid.CreateVersion7();

    [Fact]
    public void Create_OwnerWithoutLocation_Succeeds()
    {
        var result = OrganizationMember.Create(ValidOrganizationId, ValidUserId, OrganizationRole.Owner);

        result.IsSuccess.Should().BeTrue();
        result.Value.LocationId.Should().BeNull();
    }

    [Fact]
    public void Create_LocationManagerWithLocation_Succeeds()
    {
        var locationId = Guid.CreateVersion7();

        var result = OrganizationMember.Create(ValidOrganizationId, ValidUserId, OrganizationRole.LocationManager, locationId);

        result.IsSuccess.Should().BeTrue();
        result.Value.LocationId.Should().Be(locationId);
    }

    [Fact]
    public void Create_LocationManagerWithoutLocation_FailsWithLocationRequiredError()
    {
        var result = OrganizationMember.Create(ValidOrganizationId, ValidUserId, OrganizationRole.LocationManager);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.OrganizationMember.LocationRequiredForLocationManager);
    }

    [Theory]
    [InlineData(OrganizationRole.Owner)]
    [InlineData(OrganizationRole.Administrator)]
    [InlineData(OrganizationRole.Employee)]
    public void Create_NonLocationManagerRoleWithLocation_FailsWithLocationNotAllowedError(OrganizationRole role)
    {
        var result = OrganizationMember.Create(ValidOrganizationId, ValidUserId, role, Guid.CreateVersion7());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.OrganizationMember.LocationNotAllowedForRole);
    }

    [Fact]
    public void ChangeRole_ToAdministrator_UpdatesRoleAndClearsLocation()
    {
        var member = OrganizationMember.Create(
            ValidOrganizationId, ValidUserId, OrganizationRole.LocationManager, Guid.CreateVersion7()).Value;

        var result = member.ChangeRole(OrganizationRole.Administrator);

        result.IsSuccess.Should().BeTrue();
        member.Role.Should().Be(OrganizationRole.Administrator);
        member.LocationId.Should().BeNull();
    }
}