using BookingHub.Domain.ValueObjects;

namespace BookingHub.Domain.Entities;

/// <summary>
/// A client aggregated globally by phone number, independent of any single organization —
/// the same person can have bookings across multiple organizations under one <see cref="Client"/> record.
/// </summary>
public sealed class Client : BaseEntity
{
    private const int MaxNameLength = 200;

    public PhoneNumber Phone { get; private set; } = null!;
    public string? Name { get; private set; }
    public Email? Email { get; private set; }
    public Guid? UserId { get; private set; }

    private Client(Guid id, PhoneNumber phone, string? name, Email? email) : base(id)
    {
        Phone = phone;
        Name = name;
        Email = email;
    }

    private Client()
    {
    }

    public static Client Create(PhoneNumber phone, string? name = null, Email? email = null) =>
        new(Guid.CreateVersion7(), phone, NormalizeName(name), email);

    /// <summary>Fills in missing contact details from a later booking, without overwriting data already on file.</summary>
    public void UpdateContactInfo(string? name, Email? email)
    {
        if (Name is null)
        {
            var normalized = NormalizeName(name);
            if (normalized is not null)
                Name = normalized;
        }

        if (Email is null && email is not null)
            Email = email;
    }

    /// <summary>Links this client record to a registered user account.</summary>
    public Result LinkUser(Guid userId)
    {
        var userIdResult = Guard.NotEmpty(userId, "Client.UserIdEmpty", "UserId");
        if (userIdResult.IsFailure)
            return Result.Failure(userIdResult.Error);

        if (UserId is not null && UserId != userId)
            return Result.Failure(DomainErrors.Client.AlreadyLinkedToDifferentUser);

        UserId = userId;
        return Result.Success();
    }

    private static string? NormalizeName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= MaxNameLength ? name.Trim() : null;
}