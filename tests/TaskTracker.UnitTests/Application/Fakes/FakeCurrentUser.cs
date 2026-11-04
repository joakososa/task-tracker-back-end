using TaskTracker.Application.Common.Interfaces;

namespace TaskTracker.UnitTests.Application.Fakes;

public class FakeCurrentUser(int? userId) : ICurrentUser
{
    public int? UserId { get; } = userId;
}
