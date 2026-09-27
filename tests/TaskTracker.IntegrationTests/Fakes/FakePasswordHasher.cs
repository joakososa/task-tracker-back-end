using TaskTracker.Domain.Services;

namespace TaskTracker.IntegrationTests.Fakes;

public class FakePasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => $"hashed-{password}";
}
