namespace BookingHub.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_ValidAddress_Succeeds()
    {
        var result = Email.Create("user@example.com");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Create_MixedCaseAddress_IsNormalizedToLowercase()
    {
        var result = Email.Create("User@Example.COM");

        result.Value.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void Create_TwoAddressesDifferingOnlyByCase_AreEqual()
    {
        var first = Email.Create("user@example.com").Value;
        var second = Email.Create("USER@EXAMPLE.COM").Value;

        first.Should().Be(second);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespace_FailsWithEmptyError(string? rawValue)
    {
        var result = Email.Create(rawValue);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Email.Empty);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    [InlineData("@missing-local.com")]
    [InlineData("no-at-sign.com")]
    [InlineData("has spaces@example.com")]
    public void Create_InvalidFormat_FailsWithInvalidFormatError(string rawValue)
    {
        var result = Email.Create(rawValue);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Email.InvalidFormat);
    }

    [Fact]
    public void Create_ExceedingMaximumLength_FailsWithTooLongError()
    {
        var tooLong = $"{new string('a', 315)}@example.com"; // 327 chars total

        var result = Email.Create(tooLong);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Email.TooLong);
    }
}