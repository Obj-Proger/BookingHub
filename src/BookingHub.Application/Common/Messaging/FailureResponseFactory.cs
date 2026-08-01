namespace BookingHub.Application.Common.Messaging;

/// <summary>
/// Builds a failed <typeparamref name="TResponse"/> (always <c>Result</c> or <c>Result&lt;T&gt;</c>,
/// by construction of <c>ICommand</c>/<c>IQuery</c>) from an <see cref="Error"/> — shared by any
/// pipeline behavior that needs to short-circuit before the handler runs.
/// </summary>
internal static class FailureResponseFactory
{
    public static Func<Error, TResponse> Create<TResponse>()
    {
        if (typeof(TResponse) == typeof(Result))
            return error => (TResponse)(object)Result.Failure(error);

        var valueType = typeof(TResponse).GetGenericArguments()[0];
        var failureMethod = typeof(Result)
            .GetMethods()
            .Single(m => m.Name == nameof(Result.Failure) && m.IsGenericMethodDefinition)
            .MakeGenericMethod(valueType);

        return (Func<Error, TResponse>)Delegate.CreateDelegate(typeof(Func<Error, TResponse>), failureMethod);
    }
}