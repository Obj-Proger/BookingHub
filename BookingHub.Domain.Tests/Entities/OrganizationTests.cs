namespace BookingHub.Domain.Tests.Entities;

public class OrganizationTests
{
    [Fact]
    public void Create_ValidNameAndSlug_Succeeds()
    {
        var result = Organization.Create("Bright Smile Dental", "bright-smile-dental");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Bright Smile Dental");
        result.Value.Slug.Should().Be("bright-smile-dental");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_FailsWithNameEmptyError(string? name)
    {
        var result = Organization.Create(name, "valid-slug");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Organization.NameEmpty);
    }

    [Fact]
    public void Create_NameExceedingMaximumLength_FailsWithNameTooLongError()
    {
        var tooLong = new string('a', 201);

        var result = Organization.Create(tooLong, "valid-slug");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Organization.NameTooLong);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptySlug_FailsWithSlugEmptyError(string? slug)
    {
        var result = Organization.Create("Valid Name", slug);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Organization.SlugEmpty);
    }

    [Theory]
    [InlineData("Has-Uppercase")]
    [InlineData("has_underscore")]
    [InlineData("-leading-hyphen")]
    [InlineData("trailing-hyphen-")]
    [InlineData("double--hyphen")]
    [InlineData("has spaces")]
    public void Create_InvalidSlugFormat_FailsWithSlugInvalidFormatError(string slug)
    {
        var result = Organization.Create("Valid Name", slug);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Organization.SlugInvalidFormat);
    }

    [Fact]
    public void Create_SlugExceedingMaximumLength_FailsWithSlugInvalidFormatError()
    {
        // Format-valid on its own (single-char segments joined by single hyphens) —
        // isolates the length branch from the format branch of the same OR-condition.
        var tooLong = string.Join('-', Enumerable.Repeat("a", 60));

        var result = Organization.Create("Valid Name", tooLong);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Organization.SlugInvalidFormat);
    }

    [Fact]
    public void Rename_ValidNewName_UpdatesName()
    {
        var organization = Organization.Create("Old Name", "old-name-slug").Value;

        var result = organization.Rename("New Name");

        result.IsSuccess.Should().BeTrue();
        organization.Name.Should().Be("New Name");
    }

    [Fact]
    public void Rename_EmptyName_FailsAndLeavesNameUnchanged()
    {
        var organization = Organization.Create("Old Name", "old-name-slug").Value;

        var result = organization.Rename("");

        result.IsFailure.Should().BeTrue();
        organization.Name.Should().Be("Old Name");
    }
}