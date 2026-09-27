using TaskTracker.Application.Users;
using TaskTracker.Domain.Entities;
namespace TaskTracker.UnitTests.Application.Fakes;

public class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = [];
    public IReadOnlyList<User> Users => _users;

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return Task.FromResult(this._users.Any(u => u.Email.Equals(email, StringComparison.Ordinal)));
    }

    public void Add(User user)
    {
        this._users.Add(user);
    }
}