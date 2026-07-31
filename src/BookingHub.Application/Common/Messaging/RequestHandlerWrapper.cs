using Microsoft.Extensions.DependencyInjection;

namespace BookingHub.Application.Common.Messaging;

internal abstract class RequestHandlerWrapperBase
{
    public abstract Task<object?> Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

internal sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerWrapperBase
    where TRequest : IRequest<TResponse>
{
    public override async Task<object?> Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;

        Task<TResponse> InvokeHandler() =>
            serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>()
                .Handle(typedRequest, cancellationToken);

        var pipeline = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>()
            .Reverse()
            .Aggregate(
                (RequestHandlerDelegate<TResponse>)InvokeHandler,
                (next, behavior) => () => behavior.Handle(typedRequest, next, cancellationToken));

        return await pipeline();
    }
}