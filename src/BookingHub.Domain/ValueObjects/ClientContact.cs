namespace BookingHub.Domain.ValueObjects;

/// <summary>The contact details a client supplies at booking time.</summary>
public sealed class ClientContact : ValueObject
{
    public const int MaxNameLength = 200;

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

    /// <param name="phone">The client's phone number — the only required detail.</param>
    /// <param name="name">Optional; trimmed, and stored as null when blank.</param>
    /// <param name="email">Optional.</param>
    public static Result<ClientContact> Create(PhoneNumber phone, string? name = null, Email? email = null)
    {
        var nameResult = Guard.OptionalText(name, MaxNameLength, DomainErrors.ClientContact.NameTooLong);

        return nameResult.IsSuccess
            ? new ClientContact(phone, nameResult.Value, email)
            : Result.Failure<ClientContact>(nameResult.Error);
    }

    /// <summary>Validates contact details exactly as a guest entered them; a blank email means none.</summary>
    public static Result<ClientContact> Create(string? rawPhone, string? name, string? rawEmail)
    {
        var phoneResult = PhoneNumber.Create(rawPhone);
        if (phoneResult.IsFailure)
            return Result.Failure<ClientContact>(phoneResult.Error);

        Email? email = null;
        if (!string.IsNullOrWhiteSpace(rawEmail))
        {
            var emailResult = Email.Create(rawEmail);
            if (emailResult.IsFailure)
                return Result.Failure<ClientContact>(emailResult.Error);

            email = emailResult.Value;
        }

        return Create(phoneResult.Value, name, email);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Phone;
        yield return Name;
        yield return Email;
    }
}