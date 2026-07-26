namespace BookingHub.Domain.Enums;

/// <summary>Distinguishes the two kinds of schedule exception.</summary>
public enum ScheduleExceptionType
{
    /// <summary>The employee is unavailable for the entire day.</summary>
    DayOff = 1,

    /// <summary>The employee's hours differ from their recurring schedule on this date.</summary>
    ModifiedHours = 2
}