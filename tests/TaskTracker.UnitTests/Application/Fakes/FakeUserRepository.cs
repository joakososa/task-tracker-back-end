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

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return Task.FromResult(this._users.SingleOrDefault(u => u.Email.Equals(email, StringComparison.Ordinal)));
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return Task.FromResult(this._users.SingleOrDefault(u => u.Id == id));
    }

    // Assigns ids 1, 2, 3... like the database identity column, through the private setter as EF does.
    public void Add(User user)
    {
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, this._users.Count + 1);
        this._users.Add(user);
    }
}