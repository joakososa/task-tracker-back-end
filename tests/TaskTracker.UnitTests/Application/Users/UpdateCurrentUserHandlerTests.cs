using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Users.UpdateCurrent;
using TaskTracker.Domain.Entities;
using TaskTracker.UnitTests.Application.Fakes;
using TaskTracker.UnitTests.Fakes;

namespace TaskTracker.UnitTests.Application.Users;

public class UpdateCurrentUserHandlerTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeUnitOfWork _uow = new();

    public UpdateCurrentUserHandlerTests()
    {
        _users.Add(new User("Juan Pérez", "user@example.com", "securepassword", new FakePasswordHasher()));
    }

    private UpdateCurrentUserHandler CreateHandler(int? currentUserId) =>
        new(_users, new FakeCurrentUser(currentUserId), _uow);

    [Fact]
    public async Task HandleAsync_NewNameAndAvatar_ReturnsUpdatedUser()
    {
        var command = new UpdateCurrentUserCommand("Ana Gómez", "https://example.com/ana.png");

        var result = await CreateHandler(currentUserId: 1).HandleAsync(command, CancellationToken.None);

        Assert.Equal(1, result.Id);
        Assert.Equal("Ana Gómez", result.Name);
        Assert.Equal("https://example.com/ana.png", result.Avatar);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_SavesChangesOnce()
    {
        var command = new UpdateCurrentUserCommand("Ana Gómez", null);

        await CreateHandler(currentUserId: 1).HandleAsync(command, CancellationToken.None);

        Assert.Equal(1, _uow.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_UserNoLongerExists_ThrowsNotAuthenticatedWithoutSaving()
    {
        var command = new UpdateCurrentUserCommand("Ana Gómez", null);

        await Assert.ThrowsAsync<NotAuthenticatedException>(
            () => CreateHandler(currentUserId: 99).HandleAsync(command, CancellationToken.None));
        Assert.Equal(0, _uow.SaveChangesCalls);
    }
}
