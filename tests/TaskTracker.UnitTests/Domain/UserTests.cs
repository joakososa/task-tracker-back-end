using TaskTracker.Domain.Common;
using TaskTracker.Domain.Entities;

namespace TaskTracker.UnitTests.Domain;

public class UserTests
{
    [Theory]
    [InlineData("", "test@example.com", "PasswordHash")]
    [InlineData("   ", "test@example.com", "PasswordHash")]
    [InlineData("Name", "", "PasswordHash")]
    [InlineData("Name", "   ", "PasswordHash")]
    [InlineData("Name", "test@example.com", "")]
    [InlineData("Name", "test@example.com", "   ")]
    public void Constructor_WithBlankFields_Throws(string name, string email, string passwordHash)
    {
        var ex = Assert.Throws<DomainException>(() => new User(name, email, passwordHash));

        Assert.Equal(DomainErrors.FieldRequired, ex.Code);
    }

    [Theory]
    [InlineData("Name   ", "test@example.com")]
    [InlineData("Name", "tEsT@example.com    ")]
    public void Constructor_FieldsWithSpaceAtEnd_TrimsFields(string name, string email)
    {
        var user = new User(name, email, "PasswordHash");

        Assert.Equal(name.Trim(), user.Name);
        Assert.Equal(email.Trim().ToLowerInvariant(), user.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateProfile_WithBlankFields_Throws(string name)
    {
        var user = new User("Existing Name", "test@example.com", "PasswordHash");
        var ex = Assert.Throws<DomainException>(() => user.UpdateProfile(name, "AVATAR_URL"));

        Assert.Equal(DomainErrors.FieldRequired, ex.Code);
    }

    [Fact]
    public void UpdateProfile_NullAvatarUrl_SetsAvatarUrlToNull()
    {
        var user = new User("Existing Name", "test@example.com", "PasswordHash", "AVATAR_URL");

        user.UpdateProfile("Existing Name", null);

        Assert.Null(user.AvatarUrl);
    }
}