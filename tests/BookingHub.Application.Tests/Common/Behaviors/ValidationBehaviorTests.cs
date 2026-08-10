using BookingHub.Application.Common.Behaviors;
using FluentValidation;

namespace BookingHub.Application.Tests.Common.Behaviors;

public class ValidationBehaviorTests
{
    private sealed record SampleCommand(string? Name) : ICommand;
    private sealed record GenericSampleCommand(string? Name) : ICommand<string>;

    private sealed class SampleCommandValidator : AbstractValidator<SampleCommand>
    {
        public SampleCommandValidator() => RuleFor(c => c.Name).NotEmpty();
    }

    private sealed class GenericSampleCommandValidator : AbstractValidator<GenericSampleCommand>
    {
        public GenericSampleCommandValidator() => RuleFor(c => c.Name).NotEmpty();
    }

    private static RequestHandlerDelegate<Result> NextReturningSuccess() => () => Task.FromResult(Result.Success());

    [Fact]
    public async Task Handle_NoValidatorsRegistered_CallsNext()
    {
        var sut = new ValidationBehavior<SampleCommand, Result>([]);

        var result = await sut.Handle(new SampleCommand("valid"), NextReturningSuccess(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsNext()
    {
        var sut = new ValidationBehavior<SampleCommand, Result>([new SampleCommandValidator()]);

        var result = await sut.Handle(new SampleCommand("valid"), NextReturningSuccess(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShortCircuitsWithoutCallingNext()
    {
        var sut = new ValidationBehavior<SampleCommand, Result>([new SampleCommandValidator()]);
        var nextWasCalled = false;

        Task<Result> Next()
        {
            nextWasCalled = true;
            return Task.FromResult(Result.Success());
        }

        var result = await sut.Handle(new SampleCommand(null), Next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        nextWasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_MultipleFailingValidators_CombinesAllMessages()
    {
        var sut = new ValidationBehavior<SampleCommand, Result>([new SampleCommandValidator(), new SampleCommandValidator()]);

        var result = await sut.Handle(new SampleCommand(null), NextReturningSuccess(), CancellationToken.None);

        result.Error.Message.Should().Contain("must not be empty");
    }

    [Fact]
    public async Task Handle_GenericCommandInvalid_BuildsGenericResultFailure()
    {
        var sut = new ValidationBehavior<GenericSampleCommand, Result<string>>([new GenericSampleCommandValidator()]);

        var result = await sut.Handle(
            new GenericSampleCommand(null), () => Task.FromResult(Result.Success("value")), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}