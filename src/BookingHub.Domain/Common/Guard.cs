namespace BookingHub.Domain.Common;

/// <summary>
/// Shared validation for entity fields that repeat the same
/// "required, trimmed, length-limited" shape across multiple entities.
/// </summary>
internal static class Guard
{
    /// <summary>Validates that a string is non-empty (after trimming) and within a maximum length.</summary>
    public static Result<string> RequiredText(string? value, int maxLength, Error emptyError, Error tooLongError)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<string>(emptyError);

        var trimmed = value.Trim();

        return trimmed.Length > maxLength
            ? Result.Failure<string>(tooLongError)
            : trimmed;
    }

    /// <summary>Validates that a referenced identifier is not <see cref="Guid.Empty"/>.</summary>
    public static Result<Guid> NotEmpty(Guid value, string errorCode, string fieldName) =>
        value == Guid.Empty
            ? Result.Failure<Guid>(Error.Validation(errorCode, $"{fieldName} is required."))
            : value;

    /// <summary>Validates that a UTC instant is strictly later than the current time.</summary>
    public static Result<DateTime> NotPast(DateTime value, DateTime utcNow, Error error) =>
        value <= utcNow ? Result.Failure<DateTime>(error) : value;
}