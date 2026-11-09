using TaskTracker.Application.Common.Interfaces;
namespace TaskTracker.UnitTests.Application.Fakes;

public class FakeHandlerWithoutResult : ICommandHandler<FakeCommand>
{
    public int HandleAsyncCalls { get; private set; }

    public Task HandleAsync(FakeCommand command, CancellationToken cancellationToken)
    {
        HandleAsyncCalls++;
        return Task.CompletedTask;
    }
}
