namespace BookingHub.Application.Features.Employees;

/// <summary>
/// Checks non-overlap between sibling recurring-schedule entries on the same day — deferred to
/// Application because it requires seeing multiple entries at once, which a single
/// RecurringSchedule instance in the domain cannot (Domain Commit 7). Half-open interval
/// semantics, matching TimeSlot.Overlaps in Domain — touching boundaries are not an overlap.
/// </summary>
internal static class RecurringScheduleOverlap
{
    public static bool Overlaps(TimeOnly startA, TimeOnly endA, TimeOnly startB, TimeOnly endB) =>
        startA < endB && startB < endA;
}