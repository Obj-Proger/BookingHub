using BookingHub.Domain.Enums;
using BookingHub.Domain.Events;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Domain.Entities;

/// <summary>
/// A client's place in line for a slot that is currently unavailable.
/// When a matching slot opens up, it is offered to the earliest waiting entry
/// with a limited confirmation window before moving on to the next one.
/// </summary>
public sealed class WaitlistEntry : BaseEntity, IAuditable
{
    public Guid OrganizationId { get; private set; }
    public Guid LocationId { get; private set; }

    /// <summary>The requested employee, or null if the client accepts any available employee.</summary>
    public Guid? EmployeeId { get; private set; }

    public Guid ServiceId { get; private set; }
    public ClientContact ClientContact { get; private set; } = null!;
    public TimeSlot DesiredWindow { get; private set; } = null!;
    public WaitlistEntryStatus Status { get; private set; }

    public Guid? OfferedEmployeeId { get; private set; }
    public TimeSlot? OfferedSlot { get; private set; }
    public DateTime? OfferExpiresAtUtc { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }

    /// <summary>Lets the guest act on this entry (e.g. confirm an offer) without an account, mirroring <c>Booking</c>.</summary>
    public SecurityToken ManagementToken { get; private set; } = null!;

    /// <inheritdoc />
    public DateTime CreatedAtUtc { get; private set; }

    /// <inheritdoc />
    public DateTime? ModifiedAtUtc { get; private set; }

    private WaitlistEntry(
        Guid id, Guid organizationId, Guid locationId, Guid? employeeId, Guid serviceId,
        ClientContact clientContact, TimeSlot desiredWindow, SecurityToken managementToken)
        : base(id)
    {
        OrganizationId = organizationId;
        LocationId = locationId;
        EmployeeId = employeeId;
        ServiceId = serviceId;
        ClientContact = clientContact;
        DesiredWindow = desiredWindow;
        ManagementToken = managementToken;
        Status = WaitlistEntryStatus.Waiting;
    }

    private WaitlistEntry()
    {
    }

    public static Result<WaitlistEntry> Create(
        Guid organizationId, Guid locationId, Guid? employeeId, Guid serviceId,
        ClientContact clientContact, TimeSlot desiredWindow, DateTime utcNow)
    {
        var organizationIdResult = Guard.NotEmpty(organizationId, "WaitlistEntry.OrganizationIdEmpty", "OrganizationId");
        if (organizationIdResult.IsFailure)
            return Result.Failure<WaitlistEntry>(organizationIdResult.Error);

        var locationIdResult = Guard.NotEmpty(locationId, "WaitlistEntry.LocationIdEmpty", "LocationId");
        if (locationIdResult.IsFailure)
            return Result.Failure<WaitlistEntry>(locationIdResult.Error);

        if (employeeId is not null)
        {
            var employeeIdResult = Guard.NotEmpty(employeeId.Value, "WaitlistEntry.EmployeeIdEmpty", "EmployeeId");
            if (employeeIdResult.IsFailure)
                return Result.Failure<WaitlistEntry>(employeeIdResult.Error);
        }

        var serviceIdResult = Guard.NotEmpty(serviceId, "WaitlistEntry.ServiceIdEmpty", "ServiceId");
        if (serviceIdResult.IsFailure)
            return Result.Failure<WaitlistEntry>(serviceIdResult.Error);

        var windowResult = Guard.NotPast(desiredWindow.StartUtc, utcNow, DomainErrors.WaitlistEntry.SlotInPast);
        if (windowResult.IsFailure)
            return Result.Failure<WaitlistEntry>(windowResult.Error);

        return new WaitlistEntry(
            Guid.CreateVersion7(), organizationId, locationId, employeeId, serviceId,
            clientContact, desiredWindow, SecurityToken.Generate());
    }

    /// <summary>Offers a freed-up slot to this entry, starting its confirmation window.</summary>
    public Result Offer(Guid offeredEmployeeId, TimeSlot offeredSlot, DateTime offerExpiresAtUtc, DateTime utcNow)
    {
        if (Status != WaitlistEntryStatus.Waiting)
            return Result.Failure(DomainErrors.WaitlistEntry.CannotOffer);

        var employeeIdResult = Guard.NotEmpty(offeredEmployeeId, "WaitlistEntry.OfferedEmployeeIdEmpty", "OfferedEmployeeId");
        if (employeeIdResult.IsFailure)
            return Result.Failure(employeeIdResult.Error);

        var slotResult = Guard.NotPast(offeredSlot.StartUtc, utcNow, DomainErrors.WaitlistEntry.SlotInPast);
        if (slotResult.IsFailure)
            return Result.Failure(slotResult.Error);

        if (!DesiredWindow.Overlaps(offeredSlot))
            return Result.Failure(DomainErrors.WaitlistEntry.OfferOutsideDesiredWindow);

        Status = WaitlistEntryStatus.Offered;
        OfferedEmployeeId = offeredEmployeeId;
        OfferedSlot = offeredSlot;
        OfferExpiresAtUtc = offerExpiresAtUtc;

        RaiseDomainEvent(new WaitlistSlotOfferedEvent(Id, OrganizationId, ClientContact, offeredSlot, offerExpiresAtUtc, utcNow));
        return Result.Success();
    }

    /// <summary>Confirms the offer — the Application layer creates the resulting Booking from <see cref="OfferedEmployeeId"/>/<see cref="OfferedSlot"/>.</summary>
    public Result Convert(DateTime utcNow)
    {
        if (Status != WaitlistEntryStatus.Offered)
            return Result.Failure(DomainErrors.WaitlistEntry.CannotConvert);

        Status = WaitlistEntryStatus.Converted;
        ResolvedAtUtc = utcNow;
        return Result.Success();
    }

    /// <summary>Releases an unconfirmed offer after its window elapses.</summary>
    public Result Expire(DateTime utcNow)
    {
        if (Status != WaitlistEntryStatus.Offered)
            return Result.Failure(DomainErrors.WaitlistEntry.CannotExpire);

        if (OfferExpiresAtUtc > utcNow)
            return Result.Failure(DomainErrors.WaitlistEntry.OfferNotYetExpired);

        Status = WaitlistEntryStatus.Expired;
        ResolvedAtUtc = utcNow;

        RaiseDomainEvent(new WaitlistOfferExpiredEvent(
            Id, OrganizationId, LocationId, ServiceId, OfferedEmployeeId!.Value, OfferedSlot!, utcNow));
        return Result.Success();
    }

    /// <summary>The client voluntarily leaves the queue — either before any offer, or by declining one already made.</summary>
    public Result Cancel(DateTime utcNow)
    {
        if (Status is not (WaitlistEntryStatus.Waiting or WaitlistEntryStatus.Offered))
            return Result.Failure(DomainErrors.WaitlistEntry.CannotCancel);

        Status = WaitlistEntryStatus.Cancelled;
        ResolvedAtUtc = utcNow;
        return Result.Success();
    }
}