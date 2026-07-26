namespace BookingHub.Domain.ValueObjects;

/// <summary>A monetary amount in a specific currency.</summary>
public sealed class Money : ValueObject
{
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

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            return Result.Failure<Money>(DomainErrors.Money.InvalidCurrency);

        return new Money(amount, currency.Trim().ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount} {Currency}";
}