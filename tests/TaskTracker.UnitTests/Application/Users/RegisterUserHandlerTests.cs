using TaskTracker.Application.Users.Register;
using TaskTracker.UnitTests.Application.Fakes;
using TaskTracker.UnitTests.Fakes;
using TaskTracker.Domain.Common;
using TaskTracker.Domain.Entities;

namespace TaskTracker.UnitTests.Application.Users;

public class RegisterUserHandlerTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeUnitOfWork _uow = new();

    private RegisterUserHandler CreateHandler() => new(_users, _hasher, _uow);

    [Fact]
    public async Task HandleAsync_EmailAlreadyRegisteredWithDifferentFormatting_ThrowsEmailAlreadyExists()
    {
        _users.Add(new User("Existing", "user@example.com", "password", _hasher));
        var command = new RegisterUserCommand("Juan Pérez", " User@Example.COM ", "securepassword", null);

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => CreateHandler().HandleAsync(command, CancellationToken.None));

        Assert.Equal("USER_EMAIL_ALREADY_EXISTS", ex.Code);
        Assert.Equal(0, _uow.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_NewEmail_StoresHashedPassword()
    {
        var command = new RegisterUserCommand("Juan Pérez", "user@example.com", "securepassword", null);

        await CreateHandler().HandleAsync(command, CancellationToken.None);

        var stored = Assert.Single(_users.Users);
        Assert.Equal("hashed-securepassword", stored.PasswordHash);
    }

    [Fact]
    public async Task HandleAsync_NewEmail_SavesChangesOnce()
    {
        var command = new RegisterUserCommand("Juan Pérez", "user@example.com", "securepassword", null);

        await CreateHandler().HandleAsync(command, CancellationToken.None);

        Assert.Equal(1, _uow.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_NewEmail_ReturnsNormalizedUserData()
    {
        var command = new RegisterUserCommand(" Juan Pérez ", " User@Example.COM ", "securepassword", " https://example.com/a.jpg ");

        var result = await CreateHandler().HandleAsync(command, CancellationToken.None);

        Assert.Equal("Juan Pérez", result.Name);
        Assert.Equal("user@example.com", result.Email);
        Assert.Equal("https://example.com/a.jpg", result.Avatar);
    }
}