using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Users.GetCurrent;
using TaskTracker.Domain.Entities;
using TaskTracker.UnitTests.Application.Fakes;
using TaskTracker.UnitTests.Fakes;

namespace TaskTracker.UnitTests.Application.Users;

public class GetCurrentUserHandlerTests
{
    private readonly FakeUserRepository _users = new();

    public GetCurrentUserHandlerTests()
    {
        _users.Add(new User("Juan Pérez", "user@example.com", "securepassword", new FakePasswordHasher()));
    }

    private GetCurrentUserHandler CreateHandler(int? currentUserId) => new(_users, new FakeCurrentUser(currentUserId));

    [Fact]
    public async Task HandleAsync_AuthenticatedUser_ReturnsTheirData()
    {
        var result = await CreateHandler(currentUserId: 1).HandleAsync(new GetCurrentUserQuery(), CancellationToken.None);

        Assert.Equal(1, result.Id);
        Assert.Equal("Juan Pérez", result.Name);
        Assert.Equal("user@example.com", result.Email);
    }

    [Fact]
    public async Task HandleAsync_UserNoLongerExists_ThrowsNotAuthenticated()
    {
        await Assert.ThrowsAsync<NotAuthenticatedException>(
            () => CreateHandler(currentUserId: 99).HandleAsync(new GetCurrentUserQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_AnonymousRequest_ThrowsNotAuthenticated()
    {
        await Assert.ThrowsAsync<NotAuthenticatedException>(
            () => CreateHandler(currentUserId: null).HandleAsync(new GetCurrentUserQuery(), CancellationToken.None));
    }
}
