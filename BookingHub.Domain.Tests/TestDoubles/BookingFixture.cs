namespace BookingHub.Domain.Tests.TestDoubles;

/// <summary>Builds a <see cref="Booking"/> already in a given lifecycle state, with a clean domain-event list.</summary>
internal static class BookingFixture
{
    public static readonly Guid OrganizationId = Guid.CreateVersion7();
    public static readonly Guid LocationId = Guid.CreateVersion7();
    public static readonly Guid EmployeeId = Guid.CreateVersion7();
    public static readonly Guid ServiceId = Guid.CreateVersion7();
    public static readonly ClientContact ClientContact = ClientContact.Create(PhoneNumber.Create("+14155552671").Value);

    public static readonly DateTime UtcNow = new(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc);
    public static readonly TimeSlot FutureSlot = TimeSlot.Create(UtcNow.AddHours(1), UtcNow.AddHours(2)).Value;

    public static Result<Booking> CreatePendingResult(TimeSlot? timeSlot = null) =>
        Booking.CreatePending(
            OrganizationId, LocationId, EmployeeId, ServiceId, ClientContact, timeSlot ?? FutureSlot, BookingSource.Public, UtcNow);

    public static Booking CreatePending() => CreatePendingResult().Value;

    public static Booking CreateConfirmed()
    {
        var booking = CreatePending();
        booking.Confirm(UtcNow);
        booking.ClearDomainEvents();
        return booking;
    }

    public static Booking CreateAwaitingReview()
    {
        var booking = CreateConfirmed();
        booking.TransitionToAwaitingReview(FutureSlot.EndUtc.AddMinutes(1));
        booking.ClearDomainEvents();
        return booking;
    }

    public static Booking CreateCancelled()
    {
        var booking = CreateConfirmed();
        booking.Cancel(null, UtcNow);
        booking.ClearDomainEvents();
        return booking;
    }
}