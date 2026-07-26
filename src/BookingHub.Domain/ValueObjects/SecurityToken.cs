using System.Security.Cryptography;

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
        var urlSafe = Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return new SecurityToken(urlSafe);
    }

    /// <summary>Reconstructs a token from a value already persisted in the database.</summary>
    public static SecurityToken FromExisting(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}