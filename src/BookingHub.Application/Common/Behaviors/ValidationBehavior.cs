using System.Reflection;
using BookingHub.Application.Common.Messaging;
using FluentValidation;

namespace BookingHub.Application.Common.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly Func<Error, TResponse> BuildFailure = CreateFailureBuilder();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var failureMessages = new List<string>();
        foreach (var validator in validators)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            failureMessages.AddRange(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        return failureMessages.Count == 0
            ? await next()
            : BuildFailure(Error.Validation("Validation.Failed", string.Join(" ", failureMessages)));
    }

    private static Func<Error, TResponse> CreateFailureBuilder()
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