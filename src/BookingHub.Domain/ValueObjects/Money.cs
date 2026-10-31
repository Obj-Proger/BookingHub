namespace BookingHub.Domain.ValueObjects;

/// <summary>A monetary amount in a specific currency.</summary>
public sealed class Money : ValueObject
{
    public const int CurrencyCodeLength = 3;

    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <param name="amount">The monetary amount. Must be zero or positive.</param>
    /// <param name="currency">A 3-letter ISO 4217 currency code, e.g. <c>USD</c>.</param>
    public static Result<Money> Create(decimal amount, string? currency)
    {
        if (amount < 0)
            return Result.Failure<Money>(DomainErrors.Money.NegativeAmount);

        var code = currency?.Trim();
        if (code is not { Length: CurrencyCodeLength } || !code.All(char.IsAsciiLetter))
            return Result.Failure<Money>(DomainErrors.Money.InvalidCurrency);

        return new Money(amount, code.ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount} {Currency}";
}