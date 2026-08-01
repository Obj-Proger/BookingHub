using BookingHub.Application.Common.Messaging;
using FluentValidation;

namespace BookingHub.Application.Common.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly Func<Error, TResponse> BuildFailure = FailureResponseFactory.Create<TResponse>();

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
}