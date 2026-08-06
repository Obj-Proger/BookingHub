using Microsoft.Extensions.DependencyInjection;

namespace BookingHub.Application.Tests.Common.Messaging;

public class DomainEventDispatcherTests
{
    private sealed record TestEvent(DateTime OccurredOnUtc) : IDomainEvent;

    private sealed class RecordingHandler(List<string> log, string name) : IDomainEventHandler<TestEvent>
    {
        public Task Handle(TestEvent domainEvent, CancellationToken cancellationToken)
        {
            log.Add(name);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task DispatchAsync_NoHandlersRegistered_CompletesWithoutThrowing()
    {
        var dispatcher = new DomainEventDispatcher(new ServiceCollection().BuildServiceProvider());

        var act = async () => await dispatcher.DispatchAsync([new TestEvent(DateTime.UtcNow)], CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DispatchAsync_MultipleHandlersForSameEvent_InvokesAll()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddScoped<IDomainEventHandler<TestEvent>>(_ => new RecordingHandler(log, "First"));
        services.AddScoped<IDomainEventHandler<TestEvent>>(_ => new RecordingHandler(log, "Second"));
        var dispatcher = new DomainEventDispatcher(services.BuildServiceProvider());

        await dispatcher.DispatchAsync([new TestEvent(DateTime.UtcNow)], CancellationToken.None);

        log.Should().Equal("First", "Second");
    }

    [Fact]
    public async Task DispatchAsync_MultipleEvents_DispatchesEachToItsOwnHandlers()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddScoped<IDomainEventHandler<TestEvent>>(_ => new RecordingHandler(log, "Handled"));
        var dispatcher = new DomainEventDispatcher(services.BuildServiceProvider());

        await dispatcher.DispatchAsync([new TestEvent(DateTime.UtcNow), new TestEvent(DateTime.UtcNow)], CancellationToken.None);

        log.Should().Equal("Handled", "Handled");
    }
}