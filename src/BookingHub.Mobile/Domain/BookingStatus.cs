namespace BookingHub.Mobile.Domain;

/// <summary>
/// Own copy, not a reference to BookingHub.Domain — same principle as every DTO in this
/// project: Mobile only knows the wire contract, never the server's actual types. Values must
/// match the server's enum names exactly, since both sides serialize/deserialize by name.
/// </summary>
public enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    AwaitingReview = 3,
    Completed = 4,
    NoShow = 5,
    Cancelled = 6,
    Expired = 7
}