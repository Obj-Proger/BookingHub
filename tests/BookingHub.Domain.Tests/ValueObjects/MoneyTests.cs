namespace BookingHub.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_ValidAmountAndCurrency_Succeeds()
    {
        var result = Money.Create(19.99m, "usd");

        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(19.99m);
        result.Value.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_ZeroAmount_Succeeds()
    {
        var result = Money.Create(0m, "USD");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_NegativeAmount_FailsWithNegativeAmountError()
    {
        var result = Money.Create(-1m, "USD");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Money.NegativeAmount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Create_InvalidCurrency_FailsWithInvalidCurrencyError(string? currency)
    {
        var result = Money.Create(10m, currency);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Money.InvalidCurrency);
    }

    [Fact]
    public void Equals_SameAmountAndCurrency_ReturnsTrue()
    {
        var first = Money.Create(10m, "USD").Value;
        var second = Money.Create(10m, "USD").Value;

        first.Should().Be(second);
    }

    [Fact]
    public void Equals_SameAmountDifferentCurrency_ReturnsFalse()
    {
        var first = Money.Create(10m, "USD").Value;
        var second = Money.Create(10m, "EUR").Value;

        first.Should().NotBe(second);
    }
}