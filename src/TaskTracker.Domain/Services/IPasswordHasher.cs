namespace TaskTracker.Domain.Services;

/// Hashes and verifies plain-text passwords. Implemented in Infrastructure; passed to User so the entity owns its
/// password rules (ADR 0008).
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool Verify(string hash, string password);
}
