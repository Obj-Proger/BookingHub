namespace BookingHub.Domain.ValueObjects;

/// <summary>The contact details a client supplies at booking time.</summary>
public sealed class ClientContact : ValueObject
{
    public PhoneNumber Phone { get; }
    public string? Name { get; }
    public Email? Email { get; }

    private ClientContact(PhoneNumber phone, string? name, Email? email)
    {
        Phone = phone;
        Name = name;
        Email = email;
    }

    /// <param name="phone">The client's phone number. Required — the caller must supply an already-validated <see cref="PhoneNumber"/>.</param>
    /// <param name="name">The client's name, if provided.</param>
    /// <param name="email">The client's email, if provided — must already be a validated <see cref="ValueObjects.Email"/>.</param>
    public static ClientContact Create(PhoneNumber phone, string? name = null, Email? email = null)
    {
        var trimmedName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        return new ClientContact(phone, trimmedName, email);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Phone;
        yield return Name;
        yield return Email;
    }
}