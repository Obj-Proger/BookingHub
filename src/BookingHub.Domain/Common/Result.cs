namespace BookingHub.Domain.Common;

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// Use <see cref="Result{TValue}"/> when the operation produces a value on success.
/// </summary>
/// <remarks>
/// Prefer returning <see cref="Result"/> over throwing exceptions for expected domain errors.
/// Reserve exceptions for truly exceptional, unrecoverable situations.
/// </remarks>
public class Result
{
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="error">The error associated with a failed result.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a successful result is given a non-empty error,
    /// or a failed result is given an empty error.
    /// </exception>
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot carry an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failed result must carry a non-empty error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the error associated with this result. Returns <see cref="Error.None"/> on success.</summary>
    public Error Error { get; }

    /// <summary>Creates a successful result with no value.</summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>Creates a failed result with the specified error.</summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>Creates a successful result carrying the specified value.</summary>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>Creates a failed result of the specified value type.</summary>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// Represents the outcome of an operation that returns a value of type <typeparamref name="TValue"/>.
/// </summary>
/// <typeparam name="TValue">The type of value produced on success.</typeparam>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>Gets the value produced by the successful operation.</summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing the value of a failed result.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Cannot access the value of a failed result. Error: [{Error.Code}] {Error.Message}");

    /// <summary>
    /// Implicitly converts a value to a successful <see cref="Result{TValue}"/>,
    /// allowing methods returning <c>Result&lt;T&gt;</c> to simply return a <c>T</c>.
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);
}