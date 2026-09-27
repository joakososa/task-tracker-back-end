using TaskTracker.Domain.Common;
using TaskTracker.Domain.Entities;
using TaskTracker.UnitTests.Fakes;

namespace TaskTracker.UnitTests.Domain;

public class UserTests
{
    private static readonly FakePasswordHasher Hasher = new();

    [Fact]
    public void Constructor_WithNameOverMaxLength_ThrowsFieldTooLong()
    {
        var name = new string('a', User.NameMaxLength + 1);

        var ex = Assert.Throws<DomainException>(() => new User(name, "test@example.com", "password", Hasher));

        Assert.Equal("FIELD_TOO_LONG", ex.Code);
    }

    [Fact]
    public void Constructor_WithEmailOverMaxLength_ThrowsFieldTooLong()
    {
        var email = new string('a', User.EmailMaxLength + 1);

        var ex = Assert.Throws<DomainException>(() => new User("Name", email, "password", Hasher));

        Assert.Equal("FIELD_TOO_LONG", ex.Code);
    }

    [Fact]
    public void Constructor_WithAvatarUrlOverMaxLength_ThrowsFieldTooLong()
    {
        var avatar = new string('a', User.AvatarUrlMaxLength + 1);

        var ex = Assert.Throws<DomainException>(() => new User("Name", "test@example.com", "password", Hasher, avatar));

        Assert.Equal("FIELD_TOO_LONG", ex.Code);
    }

    [Fact]
    public void Constructor_WithBlankAvatarUrl_StoresNull()
    {
        var user = new User("Name", "test@example.com", "password", Hasher, "   ");

        Assert.Null(user.AvatarUrl);
    }
    [Theory]
    [InlineData("Name   ", "test@example.com")]
    [InlineData("Name", "tEsT@example.com    ")]
    public void Constructor_FieldsWithSpaceAtEnd_TrimsFields(string name, string email)
    {
        var user = new User(name, email, "password", Hasher);

        Assert.Equal(name.Trim(), user.Name);
        Assert.Equal(email.Trim().ToLowerInvariant(), user.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateProfile_WithBlankFields_Throws(string name)
    {
        var user = new User("Existing Name", "test@example.com", "password", Hasher);
        var ex = Assert.Throws<DomainException>(() => user.UpdateProfile(name, "AVATAR_URL"));

        Assert.Equal("FIELD_REQUIRED", ex.Code);
    }

    [Fact]
    public void UpdateProfile_NullAvatarUrl_SetsAvatarUrlToNull()
    {
        var user = new User("Existing Name", "test@example.com", "password", Hasher, "AVATAR_URL");

        user.UpdateProfile("Existing Name", null);

        Assert.Null(user.AvatarUrl);
    }

    [Fact]
    public void Constructor_WithPassword_StoresHashNotPlainText()
    {
        var user = new User("Name", "test@example.com", "securepassword", Hasher);

        Assert.Equal("hashed-securepassword", user.PasswordHash);
    }

    [Fact]
    public void Constructor_WithPasswordOfMinLength_CreatesUser()
    {
        var user = new User("Name", "test@example.com", "12345678", Hasher);

        Assert.Equal("hashed-12345678", user.PasswordHash);
    }

    [Fact]
    public void Constructor_WithPasswordShorterThanMin_ThrowsPasswordTooShort()
    {
        var ex = Assert.Throws<DomainException>(() => new User("Name", "test@example.com", "1234567", Hasher));

        Assert.Equal("PASSWORD_TOO_SHORT", ex.Code);
    }

    [Fact]
    public void Constructor_WithPasswordLongerThanMax_ThrowsPasswordTooLong()
    {
        var ex = Assert.Throws<DomainException>(() => new User("Name", "test@example.com", new string('a', 129), Hasher));

        Assert.Equal("PASSWORD_TOO_LONG", ex.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Constructor_WithMissingPassword_ThrowsFieldRequired(string? password)
    {
        var ex = Assert.Throws<DomainException>(() => new User("Name", "test@example.com", password!, Hasher));

        Assert.Equal("FIELD_REQUIRED", ex.Code);
    }
}
