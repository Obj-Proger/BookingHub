namespace BookingHub.Domain.Tests.TestDoubles;

/// <summary>Ready-made valid domain objects for tests that need one but aren't testing its own construction.</summary>
internal static class Fixtures
{
    public static WeeklyHours ValidWeeklyHours() =>
        WeeklyHours.Create(Enum.GetValues<DayOfWeek>().Select(DailyHours.CreateClosed)).Value;

    public static Address ValidAddress() => Address.Create("221B Baker Street, London").Value;
}