using BookingHub.Application.Common.Behaviors;
using Microsoft.Extensions.Logging;

namespace BookingHub.Application.Tests.Common.Behaviors;

public class LoggingBehaviorTests
{
    internal sealed record SampleRequest : IRequest<Result>;

    private readonly ILogger<LoggingBehavior<SampleRequest, Result>> _logger =
        Substitute.For<ILogger<LoggingBehavior<SampleRequest, Result>>>();

    [Fact]
    public async Task Handle_SuccessfulNext_ReturnsResultUnchanged()
    {
        var sut = new LoggingBehavior<SampleRequest, Result>(_logger);

        var result = await sut.Handle(new SampleRequest(), () => Task.FromResult(Result.Success()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FailedNext_ReturnsFailureUnchanged()
    {
        var sut = new LoggingBehavior<SampleRequest, Result>(_logger);
        var error = Error.Validation("Test.Invalid", "Invalid.");

        var result = await sut.Handle(new SampleRequest(), () => Task.FromResult(Result.Failure(error)), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}