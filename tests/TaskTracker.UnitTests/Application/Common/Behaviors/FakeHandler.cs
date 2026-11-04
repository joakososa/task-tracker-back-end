using TaskTracker.Application.Common.Interfaces;
namespace TaskTracker.UnitTests.Application.Fakes;

public class FakeHandler : ICommandHandler<FakeCommand, FakeDto>
{
    public int HandleAsyncCalls { get; private set; } = 0;

    public Task<FakeDto> HandleAsync(FakeCommand command, CancellationToken ct)
    {
        HandleAsyncCalls++;
        return Task.FromResult(new FakeDto("a", "b"));
    }
}