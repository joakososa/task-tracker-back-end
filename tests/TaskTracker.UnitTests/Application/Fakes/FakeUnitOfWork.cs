using TaskTracker.Application.Common.Interfaces;
namespace TaskTracker.UnitTests.Application.Fakes;

public class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCalls++;
        return Task.CompletedTask;
    }
}