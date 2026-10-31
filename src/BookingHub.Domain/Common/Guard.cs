namespace BookingHub.Domain.Common;

/// <summary>
/// Shared structural checks that repeat across entities and value objects. Every check reports
/// a failure through an <see cref="Error"/> from <see cref="DomainErrors"/> supplied by the caller,
/// so each failure keeps the code of the concept it belongs to.
/// </summary>
internal static class Guard
{
    /// <summary>Requires non-blank text and returns it trimmed, within <paramref name="maxLength"/> characters.</summary>
    public static Result<string> RequiredText(string? value, int maxLength, Error emptyError, Error tooLongError)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<string>(emptyError);

        var trimmed = value.Trim();

        return trimmed.Length > maxLength
            ? Result.Failure<string>(tooLongError)
            : trimmed;
    }

    /// <summary>Returns the text trimmed, or null when it is blank, within <paramref name="maxLength"/> characters.</summary>
    public static Result<string?> OptionalText(string? value, int maxLength, Error tooLongError)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Success<string?>(null);

        var trimmed = value.Trim();

        return trimmed.Length > maxLength
            ? Result.Failure<string?>(tooLongError)
            : Result.Success<string?>(trimmed);
    }

    /// <summary>Requires a referenced identifier other than <see cref="Guid.Empty"/>.</summary>
    public static Result<Guid> NotEmpty(Guid value, Error error) =>
        value == Guid.Empty ? Result.Failure<Guid>(error) : value;

    /// <summary>Requires a UTC instant strictly later than <paramref name="utcNow"/>.</summary>
    public static Result<DateTime> NotPast(DateTime value, DateTime utcNow, Error error) =>
        value <= utcNow ? Result.Failure<DateTime>(error) : value;
}