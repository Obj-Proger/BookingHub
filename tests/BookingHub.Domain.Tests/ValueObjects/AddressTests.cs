namespace BookingHub.Domain.Tests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Create_ValidValue_Succeeds()
    {
        var result = Address.Create("221B Baker Street, London");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("221B Baker Street, London");
    }

    [Fact]
    public void Create_ValueWithSurroundingWhitespace_IsTrimmed()
    {
        var result = Address.Create("   221B Baker Street   ");

        result.Value.Value.Should().Be("221B Baker Street");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespace_FailsWithEmptyError(string? rawValue)
    {
        var result = Address.Create(rawValue);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Address.Empty);
    }

    [Fact]
    public void Create_ExceedingMaximumLength_FailsWithTooLongError()
    {
        var tooLong = new string('a', 501);

        var result = Address.Create(tooLong);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Address.TooLong);
    }
}