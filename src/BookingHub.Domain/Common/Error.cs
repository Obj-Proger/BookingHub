namespace BookingHub.Domain.Common;

/// <summary>
/// Classifies an <see cref="Error"/> so the API layer can map it to the correct
/// HTTP status code without inspecting <see cref="Error.Code"/>.
/// </summary>
public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}

/// <summary>
/// Represents a domain error with a machine-readable code, a human-readable message,
/// and a type used for HTTP status mapping at the API boundary.
/// Errors are used as the failure state inside <see cref="Result"/> and <see cref="Result{TValue}"/>.
/// </summary>
/// <param name="Code">
/// Machine-readable error identifier following the convention <c>Concept.ErrorName</c>,
/// e.g. <c>Booking.NotFound</c> or <c>Email.InvalidFormat</c>.
/// </param>
/// <param name="Message">Human-readable error description.</param>
/// <param name="Type">The category of failure.</param>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    /// <summary>Represents the absence of an error. Used for successful results.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    /// <summary>Creates a generic failure error.</summary>
    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);

    /// <summary>Creates a validation error.</summary>
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    /// <summary>Creates an error indicating the requested resource does not exist.</summary>
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    /// <summary>Creates an error indicating the operation conflicts with the current state.</summary>
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    /// <summary>Creates an error indicating the caller is not authenticated.</summary>
    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);

    /// <summary>Creates an error indicating the caller is authenticated but lacks permission.</summary>
    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
}