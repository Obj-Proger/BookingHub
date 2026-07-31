namespace BookingHub.Application.Common.Messaging;

/// <summary>A command that changes state and returns only success/failure.</summary>
public interface ICommand : IRequest<Result>;

/// <summary>A command that changes state and returns a value on success.</summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;

public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;