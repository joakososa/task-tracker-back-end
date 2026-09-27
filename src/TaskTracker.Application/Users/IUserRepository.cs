using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Users;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
    void Add(User user);
}