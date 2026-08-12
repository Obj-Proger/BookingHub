using BookingHub.Domain.Enums;
using BookingHub.Domain.Events;
using BookingHub.Domain.ValueObjects;

namespace BookingHub.Domain.Entities;

/// <summary>
/// The central aggregate of the scheduling domain: a client's reservation of
/// one employee's time for one service, for a specific time slot.
/// </summary>
public sealed class Booking : BaseEntity, IAuditable
{
    public Guid OrganizationId { get; private set; }
    public Guid LocationId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Guid? ClientId { get; private set; }
    public ClientContact ClientContact { get; private set; } = null!;
    public TimeSlot TimeSlot { get; private set; } = null!;
    public BookingStatus Status { get; private set; }
    public BookingSource Source { get; private set; }
    public SecurityToken ConfirmationToken { get; private set; } = null!;
    public SecurityToken CancellationToken { get; private set; } = null!;
    public Guid? RecurringSeriesId { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime? ConfirmedAtUtc { get; private set; }

    /// <summary>UTC timestamp of when the booking reached its final state — check <see cref="Status"/> for which one.</summary>
    public DateTime? ResolvedAtUtc { get; private set; }

    /// <inheritdoc />
    public DateTime CreatedAtUtc { get; private set; }

    /// <inheritdoc />
    public DateTime? ModifiedAtUtc { get; private set; }

    public Money Price { get; private set; } = null!;

    private Booking(
        Guid id, Guid organizationId, Guid locationId, Guid employeeId, Guid serviceId,
        ClientContact clientContact, TimeSlot timeSlot, Money price, BookingSource source,
        SecurityToken confirmationToken, SecurityToken cancellationToken, Guid? recurringSeriesId)
        : base(id)
    {
        OrganizationId = organizationId;
        LocationId = locationId;
        EmployeeId = employeeId;
        ServiceId = serviceId;
        ClientContact = clientContact;
        TimeSlot = timeSlot;
        Price = price;
        Status = BookingStatus.Pending;
        Source = source;
        ConfirmationToken = confirmationToken;
        CancellationToken = cancellationToken;
        RecurringSeriesId = recurringSeriesId;
    }

    private Booking()
    {
    }

    /// <param name="recurringSeriesId">Set when this booking is one occurrence of a recurring series; otherwise null.</param>
    public static Result<Booking> CreatePending(
        Guid organizationId, Guid locationId, Guid employeeId, Guid serviceId,
        ClientContact clientContact, TimeSlot timeSlot, Money price, BookingSource source,
        DateTime utcNow, Guid? recurringSeriesId = null)
    {
        var organizationIdResult = Guard.NotEmpty(organizationId, "Booking.OrganizationIdEmpty", "OrganizationId");
        if (organizationIdResult.IsFailure)
            return Result.Failure<Booking>(organizationIdResult.Error);

        var locationIdResult = Guard.NotEmpty(locationId, "Booking.LocationIdEmpty", "LocationId");
        if (locationIdResult.IsFailure)
            return Result.Failure<Booking>(locationIdResult.Error);

        var employeeIdResult = Guard.NotEmpty(employeeId, "Booking.EmployeeIdEmpty", "EmployeeId");
        if (employeeIdResult.IsFailure)
            return Result.Failure<Booking>(employeeIdResult.Error);

        var serviceIdResult = Guard.NotEmpty(serviceId, "Booking.ServiceIdEmpty", "ServiceId");
        if (serviceIdResult.IsFailure)
            return Result.Failure<Booking>(serviceIdResult.Error);

        var slotResult = Guard.NotPast(timeSlot.StartUtc, utcNow, DomainErrors.Booking.SlotInPast);
        if (slotResult.IsFailure)
            return Result.Failure<Booking>(slotResult.Error);

        var booking = new Booking(
            Guid.CreateVersion7(), organizationId, locationId, employeeId, serviceId,
            clientContact, timeSlot, price, source, SecurityToken.Generate(), SecurityToken.Generate(), recurringSeriesId);

        booking.RaiseDomainEvent(new BookingCreatedEvent(
            booking.Id, organizationId, clientContact, booking.ConfirmationToken, utcNow));

        return booking;
    }

    public Result Confirm(DateTime utcNow)
    {
        if (Status != BookingStatus.Pending)
            return Result.Failure(DomainErrors.Booking.CannotConfirm);

        Status = BookingStatus.Confirmed;
        ConfirmedAtUtc = utcNow;

        RaiseDomainEvent(new BookingConfirmedEvent(Id, OrganizationId, LocationId, EmployeeId, ClientContact, TimeSlot, utcNow));
        return Result.Success();
    }

    public Result Reschedule(TimeSlot newTimeSlot, DateTime utcNow)
    {
        if (Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
            return Result.Failure(DomainErrors.Booking.CannotReschedule);

        var slotResult = Guard.NotPast(newTimeSlot.StartUtc, utcNow, DomainErrors.Booking.SlotInPast);
        if (slotResult.IsFailure)
            return Result.Failure(slotResult.Error);

        TimeSlot = newTimeSlot;

        RaiseDomainEvent(new BookingRescheduledEvent(Id, OrganizationId, LocationId, EmployeeId, ServiceId, TimeSlot, utcNow));
        return Result.Success();
    }

    /// <summary>Transitions a confirmed booking to <see cref="BookingStatus.AwaitingReview"/> once its time slot has passed.</summary>
    public Result TransitionToAwaitingReview(DateTime utcNow)
    {
        if (Status != BookingStatus.Confirmed)
            return Result.Failure(DomainErrors.Booking.CannotTransitionToAwaitingReview);

        if (TimeSlot.EndUtc > utcNow)
            return Result.Failure(DomainErrors.Booking.SlotNotYetEnded);

        Status = BookingStatus.AwaitingReview;
        return Result.Success();
    }

    public Result Complete(DateTime utcNow)
    {
        if (Status != BookingStatus.AwaitingReview)
            return Result.Failure(DomainErrors.Booking.CannotComplete);

        Status = BookingStatus.Completed;
        ResolvedAtUtc = utcNow;

        RaiseDomainEvent(new BookingCompletedEvent(Id, OrganizationId, EmployeeId, ClientContact, utcNow));
        return Result.Success();
    }

    public Result MarkNoShow(DateTime utcNow)
    {
        if (Status != BookingStatus.AwaitingReview)
            return Result.Failure(DomainErrors.Booking.CannotMarkNoShow);

        Status = BookingStatus.NoShow;
        ResolvedAtUtc = utcNow;

        RaiseDomainEvent(new NoShowRecordedEvent(Id, OrganizationId, EmployeeId, ClientContact, utcNow));
        return Result.Success();
    }

    public Result Cancel(string? reason, DateTime utcNow)
    {
        if (Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
            return Result.Failure(DomainErrors.Booking.CannotCancel);

        Status = BookingStatus.Cancelled;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        ResolvedAtUtc = utcNow;

        RaiseDomainEvent(new BookingCancelledEvent(Id, OrganizationId, LocationId, EmployeeId, ServiceId, TimeSlot, utcNow));
        return Result.Success();
    }

    /// <summary>Releases the slot automatically when the guest does not confirm the booking in time.</summary>
    public Result Expire(DateTime utcNow)
    {
        if (Status != BookingStatus.Pending)
            return Result.Failure(DomainErrors.Booking.CannotExpire);

        Status = BookingStatus.Expired;
        ResolvedAtUtc = utcNow;

        RaiseDomainEvent(new BookingExpiredEvent(Id, OrganizationId, LocationId, EmployeeId, ServiceId, TimeSlot, utcNow));
        return Result.Success();
    }

    /// <summary>Links this booking to the resolved <see cref="Client"/> record once the Application layer has found or created one by phone.</summary>
    public Result LinkClient(Guid clientId)
    {
        var clientIdResult = Guard.NotEmpty(clientId, "Booking.ClientIdEmpty", "ClientId");
        if (clientIdResult.IsFailure)
            return Result.Failure(clientIdResult.Error);

        ClientId = clientId;
        return Result.Success();
    }
}