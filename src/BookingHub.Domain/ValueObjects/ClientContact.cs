namespace BookingHub.Domain.ValueObjects;

/// <summary>The contact details a client supplies at booking time.</summary>
public sealed class ClientContact : ValueObject
{
    public PhoneNumber Phone { get; } = null!;
    public string? Name { get; }
    public Email? Email { get; }

    private ClientContact(PhoneNumber phone, string? name, Email? email)
    {
        Phone = phone;
        Name = name;
        Email = email;
    }

    /// <summary>
    /// Required by EF Core: a constructor whose parameters are themselves other complex types
    /// (Phone, Email) cannot be bound by convention (a confirmed, long-standing EF Core
    /// limitation — dotnet/efcore#26866 — not specific to this project). EF falls back to this
    /// parameterless constructor and materializes by writing directly to each property's
    /// backing field; the parameterized constructor above remains the only way domain code
    /// constructs this type.
    /// </summary>
    private ClientContact()
    {
    }

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