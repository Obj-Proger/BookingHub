namespace BookingHub.Application.Common.Messaging;

/// <summary>Single entry point for the API layer: send a command/query, get back the response.</summary>
public interface IDispatcher
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}