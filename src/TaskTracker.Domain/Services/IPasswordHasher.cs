namespace TaskTracker.Domain.Services;

/// Hashes plain-text passwords. Implemented in Infrastructure; passed to User so the entity owns its password rules.
public interface IPasswordHasher
{
    string HashPassword(string password);
}
