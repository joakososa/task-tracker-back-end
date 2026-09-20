using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Entities;

public class User : AuditableEntity
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 256;
    public const int AvatarUrlMaxLength = 2048;

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string? AvatarUrl { get; private set; }

    private User() { }

    public User(string name, string email, string passwordHash, string? avatarUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(DomainErrors.FieldRequired, "Name is required.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException(DomainErrors.FieldRequired, "Email is required.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException(DomainErrors.FieldRequired, "Password hash is required.");

        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        AvatarUrl = avatarUrl;
    }

    public void UpdateProfile(string name, string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(DomainErrors.FieldRequired, "Name cannot be empty.");
        Name = name.Trim();

        AvatarUrl = avatarUrl;
    }
}