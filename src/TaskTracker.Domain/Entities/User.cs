using TaskTracker.Domain.Common;
using TaskTracker.Domain.Services;

namespace TaskTracker.Domain.Entities;

public class User : AuditableEntity
{
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 256;
    public const int PasswordHashMaxLength = 500;
    public const int AvatarUrlMaxLength = 2048;
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 128;

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string? AvatarUrl { get; private set; }

    private User() { }

    public User(string name, string email, string password, IPasswordHasher hasher, string? avatarUrl = null)
    {
        Name = Guard.RequiredText(name, NameMaxLength, nameof(Name));
        Email = NormalizeEmail(Guard.RequiredText(email, EmailMaxLength, nameof(Email)));
        PasswordHash = HashPassword(password, hasher);
        AvatarUrl = Guard.OptionalText(avatarUrl, AvatarUrlMaxLength, nameof(AvatarUrl));
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    // Passwords are not trimmed: spaces are valid characters (NIST 800-63B).
    private static string HashPassword(string? password, IPasswordHasher hasher)
    {
        if (string.IsNullOrEmpty(password))
            throw new DomainException(DomainErrors.FieldRequired, "Password is required.");
        if (password.Length < PasswordMinLength)
            throw new DomainException(DomainErrors.User.PasswordTooShort, $"Password must have at least {PasswordMinLength} characters.");
        if (password.Length > PasswordMaxLength)
            throw new DomainException(DomainErrors.User.PasswordTooLong, $"Password cannot exceed {PasswordMaxLength} characters.");

        return Guard.RequiredText(hasher.HashPassword(password), PasswordHashMaxLength, nameof(PasswordHash));
    }

    public void UpdateProfile(string name, string? avatarUrl)
    {
        Name = Guard.RequiredText(name, NameMaxLength, nameof(Name));
        AvatarUrl = Guard.OptionalText(avatarUrl, AvatarUrlMaxLength, nameof(AvatarUrl));
    }
}