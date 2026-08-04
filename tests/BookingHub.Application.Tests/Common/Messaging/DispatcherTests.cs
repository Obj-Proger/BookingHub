using Microsoft.Extensions.DependencyInjection;

namespace BookingHub.Application.Tests.Common.Messaging;

public class DispatcherTests
{
    private sealed record PingRequest : IRequest<Result<string>>;

    private sealed class PingRequestHandler : IRequestHandler<PingRequest, Result<string>>
    {
        public Task<Result<string>> Handle(PingRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success("pong"));
    }

    private sealed class RecordingBehavior(List<string> log, string name) : IPipelineBehavior<PingRequest, Result<string>>
    {
        public async Task<Result<string>> Handle(PingRequest request, RequestHandlerDelegate<Result<string>> next, CancellationToken cancellationToken)
        {
            log.Add($"{name}:before");
            var response = await next();
            log.Add($"{name}:after");
            return response;
        }
    }

    [Fact]
    public async Task Send_RegisteredHandler_ReturnsHandlerResult()
    {
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<PingRequest, Result<string>>, PingRequestHandler>();
        var dispatcher = new Dispatcher(services.BuildServiceProvider());

        var result = await dispatcher.Send(new PingRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("pong");
    }

    [Fact]
    public async Task Send_NoHandlerRegistered_Throws()
    {
        var dispatcher = new Dispatcher(new ServiceCollection().BuildServiceProvider());

        var act = async () => await dispatcher.Send(new PingRequest());

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Send_MultipleBehaviorsRegistered_RunsThemInRegistrationOrderWrappingTheHandler()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<PingRequest, Result<string>>, PingRequestHandler>();
        services.AddScoped<IPipelineBehavior<PingRequest, Result<string>>>(_ => new RecordingBehavior(log, "First"));
        services.AddScoped<IPipelineBehavior<PingRequest, Result<string>>>(_ => new RecordingBehavior(log, "Second"));
        var dispatcher = new Dispatcher(services.BuildServiceProvider());

        await dispatcher.Send(new PingRequest());

        log.Should().Equal("First:before", "Second:before", "Second:after", "First:after");
    }
}