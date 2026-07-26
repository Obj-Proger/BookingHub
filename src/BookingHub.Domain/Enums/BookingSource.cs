namespace BookingHub.Domain.Enums;

/// <summary>Indicates how a booking originated.</summary>
public enum BookingSource
{
    /// <summary>Self-service booking made by a client on the public booking page.</summary>
    Public = 1,

    /// <summary>Created manually by staff (e.g. a walk-in or phone booking).</summary>
    StaffCreated = 2,

    /// <summary>Created automatically from a confirmed waitlist offer.</summary>
    Waitlist = 3
}