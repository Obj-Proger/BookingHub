using System.Text.RegularExpressions;

namespace BookingHub.Domain.ValueObjects;

/// <summary>A validated email address.</summary>
public sealed partial class Email : ValueObject
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex Pattern();

    /// <summary>The maximum mailbox length allowed by RFC 5321.</summary>
    public const int MaxLength = 320;

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>Validates and creates an <see cref="Email"/> from raw input.</summary>
    public static Result<Email> Create(string? rawValue)
    {
        var valueResult = Guard.RequiredText(rawValue, MaxLength, DomainErrors.Email.Empty, DomainErrors.Email.TooLong);
        if (valueResult.IsFailure)
            return Result.Failure<Email>(valueResult.Error);

        if (!Pattern().IsMatch(valueResult.Value))
            return Result.Failure<Email>(DomainErrors.Email.InvalidFormat);

        return new Email(valueResult.Value.ToLowerInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}