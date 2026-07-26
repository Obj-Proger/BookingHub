using System.Text.RegularExpressions;

namespace BookingHub.Domain.ValueObjects;

/// <summary>A validated email address.</summary>
public sealed class Email : ValueObject
{
    private static readonly Regex Pattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>Validates and creates an <see cref="Email"/> from raw input.</summary>
    public static Result<Email> Create(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return Result.Failure<Email>(DomainErrors.Email.Empty);

        var trimmed = rawValue.Trim();

        if (trimmed.Length > 320) // RFC 5321 maximum mailbox length
            return Result.Failure<Email>(DomainErrors.Email.TooLong);

        if (!Pattern.IsMatch(trimmed))
            return Result.Failure<Email>(DomainErrors.Email.InvalidFormat);

        return new Email(trimmed.ToLowerInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}