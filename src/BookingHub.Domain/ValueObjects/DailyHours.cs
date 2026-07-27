namespace BookingHub.Domain.ValueObjects;

/// <summary>A single day's operating hours, or a closed day when both times are null.</summary>
public sealed class DailyHours : ValueObject
{
    public DayOfWeek DayOfWeek { get; }
    public TimeOnly? OpenTime { get; }
    public TimeOnly? CloseTime { get; }

    public bool IsClosed => OpenTime is null;

    private DailyHours(DayOfWeek dayOfWeek, TimeOnly? openTime, TimeOnly? closeTime)
    {
        DayOfWeek = dayOfWeek;
        OpenTime = openTime;
        CloseTime = closeTime;
    }

    /// <summary>Creates a closed day — the location does not operate on this day.</summary>
    public static DailyHours CreateClosed(DayOfWeek dayOfWeek) => new(dayOfWeek, null, null);

    /// <summary>Creates an open day with the specified operating hours.</summary>
    public static Result<DailyHours> CreateOpen(DayOfWeek dayOfWeek, TimeOnly openTime, TimeOnly closeTime)
    {
        if (openTime >= closeTime)
            return Result.Failure<DailyHours>(DomainErrors.DailyHours.OpenNotBeforeClose);

        return new DailyHours(dayOfWeek, openTime, closeTime);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DayOfWeek;
        yield return OpenTime;
        yield return CloseTime;
    }
}