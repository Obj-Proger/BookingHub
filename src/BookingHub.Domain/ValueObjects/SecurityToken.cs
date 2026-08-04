using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace BookingHub.Domain.ValueObjects;

/// <summary>
/// An unpredictable, cryptographically random token used in booking
/// confirmation and cancellation links.
/// </summary>
public sealed class SecurityToken : ValueObject
{
    private const int ByteLength = 32;

    public string Value { get; }

    private SecurityToken(string value)
    {
        Value = value;
    }

    /// <summary>Generates a new, cryptographically random token.</summary>
    public static SecurityToken Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(ByteLength);
        return new SecurityToken(Base64Url.EncodeToString(bytes));
    }

    /// <summary>Reconstructs a token from a value already persisted in the database.</summary>
    public static SecurityToken FromExisting(string value) => new(value);

    /// <summary>
    /// Compares this token against a candidate in constant time, regardless of where the
    /// two values first differ — unlike the value-based <see cref="ValueObject.Equals(ValueObject)"/>
    /// this class inherits, which is not safe for comparing secrets.
    /// </summary>
    public bool Matches(SecurityToken? candidate)
    {
        if (candidate is null)
            return false;

        var expected = Encoding.UTF8.GetBytes(Value);
        var actual = Encoding.UTF8.GetBytes(candidate.Value);

        return expected.Length == actual.Length && CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}