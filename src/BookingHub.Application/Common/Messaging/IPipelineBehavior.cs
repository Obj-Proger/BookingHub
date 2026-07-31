namespace BookingHub.Application.Common.Messaging;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

public interface IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}