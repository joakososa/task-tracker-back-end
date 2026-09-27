using IdentityPasswordHasher = Microsoft.AspNetCore.Identity.PasswordHasher<TaskTracker.Domain.Entities.User>;

namespace TaskTracker.Infrastructure.Security;

/// Adapts ASP.NET Core Identity's PBKDF2 hasher (only the hasher, not the rest of Identity; see ADR 0005).
public class PasswordHasher : Domain.Services.IPasswordHasher
{
    private readonly IdentityPasswordHasher _inner = new();

    // The default Identity implementation ignores the user argument; the User is still being constructed here.
    public string HashPassword(string password) => _inner.HashPassword(null!, password);
}
