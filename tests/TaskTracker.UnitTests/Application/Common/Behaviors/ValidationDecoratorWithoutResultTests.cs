using FluentValidation;
using TaskTracker.Application.Common.Behaviors;
using TaskTracker.UnitTests.Application.Fakes;

namespace TaskTracker.UnitTests.Application.Common.Behaviors;

public class ValidationDecoratorWithoutResultTests
{
    private readonly FakeHandlerWithoutResult _inner = new();

    private ValidationDecorator<FakeCommand> CreateDecorator(params IValidator<FakeCommand>[] validators) =>
        new(_inner, validators);

    [Fact]
    public async Task HandleAsync_InvalidCommand_ThrowsValidationExceptionWithoutCallingInner()
    {
        var command = new FakeCommand("not-an-email", "B");

        await Assert.ThrowsAsync<ValidationException>(
            () => CreateDecorator(new FakeCommandValidator()).HandleAsync(command, CancellationToken.None));

        Assert.Equal(0, _inner.HandleAsyncCalls);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CallsInner()
    {
        var command = new FakeCommand("user@example.com", "B");

        await CreateDecorator(new FakeCommandValidator()).HandleAsync(command, CancellationToken.None);

        Assert.Equal(1, _inner.HandleAsyncCalls);
    }

    [Fact]
    public async Task HandleAsync_NoValidatorsRegistered_CallsInner()
    {
        var command = new FakeCommand("not-an-email", "B");

        await CreateDecorator().HandleAsync(command, CancellationToken.None);

        Assert.Equal(1, _inner.HandleAsyncCalls);
    }
}
