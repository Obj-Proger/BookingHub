using BookingHub.Domain.ValueObjects;

namespace BookingHub.Domain.Entities;

/// <summary>
/// A client aggregated globally by phone number, independent of any single organization —
/// the same person can have bookings across multiple organizations under one <see cref="Client"/> record.
/// </summary>
public sealed class Client : BaseEntity
{
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

    /// <summary>Starts a client record from the contact details given with a booking.</summary>
    public static Client Create(ClientContact contact) =>
        new(Guid.CreateVersion7(), contact.Phone, contact.Name, contact.Email);

    /// <summary>
    /// Fills in details missing on file from a later booking's contact, never overwriting data
    /// already known. The phone number is the client's identity and is not taken from <paramref name="contact"/>.
    /// </summary>
    public void UpdateContactInfo(ClientContact contact)
    {
        Name ??= contact.Name;
        Email ??= contact.Email;
    }

    /// <summary>Links this client record to a registered user account.</summary>
    public Result LinkUser(Guid userId)
    {
        var userIdResult = Guard.NotEmpty(userId, DomainErrors.Client.UserIdEmpty);
        if (userIdResult.IsFailure)
            return Result.Failure(userIdResult.Error);

        if (UserId is not null && UserId != userId)
            return Result.Failure(DomainErrors.Client.AlreadyLinkedToDifferentUser);

        UserId = userId;
        return Result.Success();
    }
}