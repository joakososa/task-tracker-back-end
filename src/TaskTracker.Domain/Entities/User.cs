using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Entities;

public class User : AuditableEntity
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 256;
    public const int PasswordHashMaxLength = 500;
    public const int AvatarUrlMaxLength = 2048;

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string? AvatarUrl { get; private set; }

    private User() { }

    public User(string name, string email, string passwordHash, string? avatarUrl = null)
    {
        Name = Guard.RequiredText(name, NameMaxLength, nameof(Name));
        Email = Guard.RequiredText(email, EmailMaxLength, nameof(Email)).ToLowerInvariant();
        PasswordHash = Guard.RequiredText(passwordHash, PasswordHashMaxLength, nameof(PasswordHash));
        AvatarUrl = Guard.OptionalText(avatarUrl, AvatarUrlMaxLength, nameof(AvatarUrl));
    }


    public void UpdateProfile(string name, string? avatarUrl)
    {
        Name = Guard.RequiredText(name, NameMaxLength, nameof(Name));
        AvatarUrl = Guard.OptionalText(avatarUrl, AvatarUrlMaxLength, nameof(AvatarUrl));
    }
}