using TaskTracker.Application.Common.Interfaces;
namespace TaskTracker.UnitTests.Application.Fakes;

public class FakeHandler : ICommandHandler<FakeCommand, FakeDto>
{
    public int HandleAsyncCalls { get; private set; }

    public Task<FakeDto> HandleAsync(FakeCommand command, CancellationToken cancellationToken)
    {
        HandleAsyncCalls++;
        return Task.FromResult(new FakeDto("a", "b"));
    }
}