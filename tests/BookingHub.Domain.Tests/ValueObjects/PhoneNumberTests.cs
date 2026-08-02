namespace BookingHub.Domain.Tests.ValueObjects;

public class PhoneNumberTests
{
    [Fact]
    public void Create_ValidE164Number_Succeeds()
    {
        var result = PhoneNumber.Create("+14155552671");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("+14155552671");
    }

    [Fact]
    public void Create_NumberWithFormattingCharacters_IsNormalized()
    {
        var result = PhoneNumber.Create("+1 (415) 555-2671");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("+14155552671");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespace_FailsWithEmptyError(string? rawValue)
    {
        var result = PhoneNumber.Create(rawValue);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.PhoneNumber.Empty);
    }

    [Theory]
    [InlineData("14155552671")]         // missing leading '+'
    [InlineData("+0123456789")]         // leading digit cannot be 0
    [InlineData("+123456")]             // too few digits
    [InlineData("+1234567890123456")]   // too many digits
    [InlineData("not-a-number")]
    public void Create_InvalidFormat_FailsWithInvalidFormatError(string rawValue)
    {
        var result = PhoneNumber.Create(rawValue);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.PhoneNumber.InvalidFormat);
    }
}