using TaskTracker.Domain.Services;

namespace TaskTracker.UnitTests.Fakes;

public class FakePasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => $"hashed-{password}";
}
