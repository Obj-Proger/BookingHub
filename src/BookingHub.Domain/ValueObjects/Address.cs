namespace BookingHub.Domain.ValueObjects;

/// <summary>A free-form postal address.</summary>
public sealed class Address : ValueObject
{
    public string Value { get; }

    private Address(string value)
    {
        Value = value;
    }

    public static Result<Address> Create(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return Result.Failure<Address>(DomainErrors.Address.Empty);

        var trimmed = rawValue.Trim();

        if (trimmed.Length > 500)
            return Result.Failure<Address>(DomainErrors.Address.TooLong);

        return new Address(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}