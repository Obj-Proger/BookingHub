namespace BookingHub.Domain.Enums;

/// <summary>The lifecycle state of a waitlist entry.</summary>
public enum WaitlistEntryStatus
{
    /// <summary>Queued, waiting for a matching slot to open up.</summary>
    Waiting = 1,

    /// <summary>A slot opened up; the client has a limited window to confirm.</summary>
    Offered = 2,

    /// <summary>The offer was confirmed and converted into a booking.</summary>
    Converted = 3,

    /// <summary>The offer window elapsed without confirmation.</summary>
    Expired = 4,

    /// <summary>The client left the waitlist voluntarily.</summary>
    Cancelled = 5
}