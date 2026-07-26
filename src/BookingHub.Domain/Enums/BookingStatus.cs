namespace BookingHub.Domain.Enums;

/// <summary>
/// The lifecycle state of a booking. Valid transitions between these states
/// are enforced by the <c>Booking</c> entity's behavior methods, not by this enum.
/// </summary>
public enum BookingStatus
{
    /// <summary>Created, awaiting guest confirmation via an SMS/email code.</summary>
    Pending = 1,

    /// <summary>Confirmed — occupies the slot and participates in double-booking protection.</summary>
    Confirmed = 2,

    /// <summary>The slot's time has passed; awaiting the employee to mark it Completed or NoShow.</summary>
    AwaitingReview = 3,

    /// <summary>The client attended and the service was provided.</summary>
    Completed = 4,

    /// <summary>The client did not attend.</summary>
    NoShow = 5,

    /// <summary>Cancelled before the appointment took place.</summary>
    Cancelled = 6,

    /// <summary>Not confirmed by the guest in time — the slot is released automatically.</summary>
    Expired = 7
}