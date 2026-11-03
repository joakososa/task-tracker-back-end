using TaskTracker.Application.Auth.Login;
using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Domain.Entities;
using TaskTracker.UnitTests.Application.Fakes;
using TaskTracker.UnitTests.Fakes;

namespace TaskTracker.UnitTests.Application.Auth;

public class LoginHandlerTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeTokenService _tokens = new();

    private LoginHandler CreateHandler() => new(_users, _hasher, _tokens);

    public LoginHandlerTests()
    {
        _users.Add(new User("Juan Pérez", "user@example.com", "securepassword", _hasher));
    }

    [Fact]
    public async Task HandleAsync_UnknownEmail_ThrowsAuthenticationFailed()
    {
        var command = new LoginCommand("nobody@example.com", "securepassword");

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => CreateHandler().HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WrongPassword_ThrowsAuthenticationFailed()
    {
        var command = new LoginCommand("user@example.com", "wrongpassword");

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => CreateHandler().HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_CorrectCredentials_ReturnsTokenFromTokenService()
    {
        var command = new LoginCommand("user@example.com", "securepassword");

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        Assert.Equal("token-for-user@example.com", result.AccessToken);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(3600, result.ExpiresIn);
    }

    [Fact]
    public async Task HandleAsync_EmailWithDifferentFormatting_ReturnsToken()
    {
        var command = new LoginCommand(" User@Example.COM ", "securepassword");

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        Assert.Equal("token-for-user@example.com", result.AccessToken);
    }
}
