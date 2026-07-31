namespace BookingHub.Application.Common.Messaging;

/// <summary>Marker for a request that expects a response of type <typeparamref name="TResponse"/>.</summary>
public interface IRequest<TResponse>;

public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}