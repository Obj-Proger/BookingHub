namespace BookingHub.Domain.ValueObjects;

/// <summary>A free-form postal address.</summary>
public sealed class Address : ValueObject
{
    public const int MaxLength = 500;

    public string Value { get; }

    private Address(string value)
    {
        Value = value;
    }

    public static Result<Address> Create(string? rawValue)
    {
        var valueResult = Guard.RequiredText(rawValue, MaxLength, DomainErrors.Address.Empty, DomainErrors.Address.TooLong);

        return valueResult.IsSuccess
            ? new Address(valueResult.Value)
            : Result.Failure<Address>(valueResult.Error);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}