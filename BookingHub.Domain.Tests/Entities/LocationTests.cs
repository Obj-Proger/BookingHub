using BookingHub.Domain.Tests.TestDoubles;

namespace BookingHub.Domain.Tests.Entities;

public class LocationTests
{
    private static readonly Guid ValidOrganizationId = Guid.CreateVersion7();

    [Fact]
    public void Create_ValidData_Succeeds()
    {
        var result = Location.Create(
            ValidOrganizationId, "Downtown Branch", Fixtures.ValidAddress(), "UTC", Fixtures.ValidWeeklyHours());

        result.IsSuccess.Should().BeTrue();
        result.Value.OrganizationId.Should().Be(ValidOrganizationId);
        result.Value.Name.Should().Be("Downtown Branch");
        result.Value.TimeZone.Should().Be("UTC");
    }

    [Fact]
    public void Create_EmptyOrganizationId_FailsWithValidationError()
    {
        var result = Location.Create(
            Guid.Empty, "Downtown Branch", Fixtures.ValidAddress(), "UTC", Fixtures.ValidWeeklyHours());

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_FailsWithNameEmptyError(string? name)
    {
        var result = Location.Create(
            ValidOrganizationId, name, Fixtures.ValidAddress(), "UTC", Fixtures.ValidWeeklyHours());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Location.NameEmpty);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Not/A/Real/Zone")]
    public void Create_InvalidTimeZone_FailsWithInvalidTimeZoneError(string? timeZone)
    {
        var result = Location.Create(
            ValidOrganizationId, "Downtown Branch", Fixtures.ValidAddress(), timeZone, Fixtures.ValidWeeklyHours());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Location.InvalidTimeZone);
    }

    [Fact]
    public void Rename_ValidNewName_UpdatesName()
    {
        var location = Location.Create(
            ValidOrganizationId, "Old Name", Fixtures.ValidAddress(), "UTC", Fixtures.ValidWeeklyHours()).Value;

        var result = location.Rename("New Name");

        result.IsSuccess.Should().BeTrue();
        location.Name.Should().Be("New Name");
    }

    [Fact]
    public void Relocate_UpdatesAddress()
    {
        var location = Location.Create(
            ValidOrganizationId, "Branch", Fixtures.ValidAddress(), "UTC", Fixtures.ValidWeeklyHours()).Value;
        var newAddress = Address.Create("742 Evergreen Terrace, Springfield").Value;

        location.Relocate(newAddress);

        location.Address.Should().Be(newAddress);
    }

    [Fact]
    public void UpdateWorkingHours_UpdatesWorkingHours()
    {
        var location = Location.Create(
            ValidOrganizationId, "Branch", Fixtures.ValidAddress(), "UTC", Fixtures.ValidWeeklyHours()).Value;
        var mondayOpen = DailyHours.CreateOpen(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(18, 0)).Value;
        var newHours = WeeklyHours.Create(
            Enum.GetValues<DayOfWeek>().Select(day => day == DayOfWeek.Monday ? mondayOpen : DailyHours.CreateClosed(day))).Value;

        location.UpdateWorkingHours(newHours);

        location.WorkingHours.Should().Be(newHours);
    }
}