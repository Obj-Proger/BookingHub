using System.Text.RegularExpressions;

namespace BookingHub.Domain.ValueObjects;

/// <summary>An international phone number in E.164 format (e.g. <c>+14155552671</c>).</summary>
public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex Pattern = new(@"^\+[1-9]\d{7,14}$", RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    /// <summary>Validates and creates a <see cref="PhoneNumber"/> from raw input.</summary>
    /// <remarks>
    /// This is a structural E.164 check only (leading <c>+</c>, 8–15 digits) —
    /// it does not verify the number against real country dialing plans.
    /// The input must already include the country code with a leading <c>+</c>;
    /// it is not inferred or added automatically.
    /// </remarks>
    public static Result<PhoneNumber> Create(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return Result.Failure<PhoneNumber>(DomainErrors.PhoneNumber.Empty);

        var normalized = Regex.Replace(rawValue.Trim(), @"[\s\-()]", "");

        if (!Pattern.IsMatch(normalized))
            return Result.Failure<PhoneNumber>(DomainErrors.PhoneNumber.InvalidFormat);

        return new PhoneNumber(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}