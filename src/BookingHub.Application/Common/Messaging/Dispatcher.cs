using System.Collections.Concurrent;

namespace BookingHub.Application.Common.Messaging;

internal sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, RequestHandlerWrapperBase> Wrappers = new();

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        var wrapper = Wrappers.GetOrAdd(requestType, reqType =>
        {
            var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(reqType, typeof(TResponse));
            return (RequestHandlerWrapperBase)Activator.CreateInstance(wrapperType)!;
        });

        var result = await wrapper.Handle(request, serviceProvider, cancellationToken);
        return (TResponse)result!;
    }
}