namespace BookingHub.Domain.Tests.TestDoubles;

internal static class WaitlistEntryFixture
{
    public static readonly Guid OrganizationId = Guid.CreateVersion7();
    public static readonly Guid LocationId = Guid.CreateVersion7();
    public static readonly Guid ServiceId = Guid.CreateVersion7();
    public static readonly ClientContact ClientContact = ClientContact.Create(PhoneNumber.Create("+14155552671").Value);

    public static readonly DateTime UtcNow = new(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc);

    public static readonly TimeSlot DesiredWindow = TimeSlot.Create(
        new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 3, 10, 18, 0, 0, DateTimeKind.Utc)).Value;

    public static readonly TimeSlot OfferedSlotWithinWindow = TimeSlot.Create(
        new DateTime(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 3, 10, 11, 0, 0, DateTimeKind.Utc)).Value;

    public static Result<WaitlistEntry> CreateWaitingResult(Guid? employeeId = null, TimeSlot? desiredWindow = null) =>
        WaitlistEntry.Create(OrganizationId, LocationId, employeeId, ServiceId, ClientContact, desiredWindow ?? DesiredWindow, UtcNow);

    public static WaitlistEntry CreateWaiting() => CreateWaitingResult().Value;

    public static WaitlistEntry CreateOffered()
    {
        var entry = CreateWaiting();
        entry.Offer(Guid.CreateVersion7(), OfferedSlotWithinWindow, UtcNow.AddMinutes(30), UtcNow);
        entry.ClearDomainEvents();
        return entry;
    }
}