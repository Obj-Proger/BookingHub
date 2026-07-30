namespace BookingHub.Domain.Tests.ValueObjects;

public class SecurityTokenTests
{
    [Fact]
    public void Generate_ProducesAUrlSafeNonEmptyValue()
    {
        var token = SecurityToken.Generate();

        token.Value.Should().NotBeNullOrEmpty();
        token.Value.Should().NotContain("+").And.NotContain("/").And.NotContain("=");
    }

    [Fact]
    public void Generate_CalledTwice_ProducesDifferentValues()
    {
        var first = SecurityToken.Generate();
        var second = SecurityToken.Generate();

        first.Should().NotBe(second);
    }

    [Fact]
    public void FromExisting_RoundTripsTheGivenValue()
    {
        var token = SecurityToken.FromExisting("abc123");

        token.Value.Should().Be("abc123");
    }
}