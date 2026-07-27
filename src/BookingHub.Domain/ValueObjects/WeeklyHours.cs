namespace BookingHub.Domain.ValueObjects;

/// <summary>A location's default operating hours for every day of the week.</summary>
public sealed class WeeklyHours : ValueObject
{
    public IReadOnlyList<DailyHours> Days { get; }

    private WeeklyHours(IReadOnlyList<DailyHours> days)
    {
        Days = days;
    }

    /// <param name="days">Must contain exactly one entry for each of the seven days of the week.</param>
    public static Result<WeeklyHours> Create(IEnumerable<DailyHours> days)
    {
        var list = days.ToList();

        if (list.Count != 7 || list.Select(d => d.DayOfWeek).Distinct().Count() != 7)
            return Result.Failure<WeeklyHours>(DomainErrors.WeeklyHours.MustCoverAllDays);

        return new WeeklyHours(list.OrderBy(d => d.DayOfWeek).ToList());
    }

    /// <summary>Gets the operating hours for the specified day.</summary>
    public DailyHours For(DayOfWeek dayOfWeek) => Days.First(d => d.DayOfWeek == dayOfWeek);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        foreach (var day in Days)
            yield return day;
    }
}