namespace BookingHub.Domain.ValueObjects;

/// <summary>A UTC time range, represented as a half-open interval [Start, End).</summary>
public sealed class TimeSlot : ValueObject
{
    public DateTime StartUtc { get; }
    public DateTime EndUtc { get; }

    private TimeSlot(DateTime startUtc, DateTime endUtc)
    {
        StartUtc = startUtc;
        EndUtc = endUtc;
    }

    /// <param name="startUtc">The start instant. Must have <see cref="DateTimeKind.Utc"/>.</param>
    /// <param name="endUtc">The end instant. Must be later than <paramref name="startUtc"/>.</param>
    public static Result<TimeSlot> Create(DateTime startUtc, DateTime endUtc)
    {
        if (startUtc.Kind != DateTimeKind.Utc || endUtc.Kind != DateTimeKind.Utc)
            return Result.Failure<TimeSlot>(DomainErrors.TimeSlot.NotUtc);

        if (startUtc >= endUtc)
            return Result.Failure<TimeSlot>(DomainErrors.TimeSlot.StartNotBeforeEnd);

        return new TimeSlot(startUtc, endUtc);
    }

    public TimeSpan Duration => EndUtc - StartUtc;

    /// <summary>Determines whether this slot overlaps with another.</summary>
    public bool Overlaps(TimeSlot other) => StartUtc < other.EndUtc && other.StartUtc < EndUtc;

    /// <summary>Determines whether the specified instant falls within this slot.</summary>
    public bool Contains(DateTime instantUtc) => instantUtc >= StartUtc && instantUtc < EndUtc;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartUtc;
        yield return EndUtc;
    }

    public override string ToString() => $"{StartUtc:O} – {EndUtc:O}";
}