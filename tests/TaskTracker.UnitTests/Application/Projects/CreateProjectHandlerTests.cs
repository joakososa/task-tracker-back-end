using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Projects.Create;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.UnitTests.Application.Fakes;
using TaskTracker.UnitTests.Fakes;

namespace TaskTracker.UnitTests.Application.Projects;

public class CreateProjectHandlerTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeProjectRepository _projects;
    private readonly FakeUnitOfWork _uow = new();

    public CreateProjectHandlerTests()
    {
        _projects = new FakeProjectRepository(_users);
        _users.Add(new User("Ana Gómez", "ana@example.com", "securepassword", new FakePasswordHasher()));
    }

    private CreateProjectHandler CreateHandler(int? currentUserId) =>
        new(_projects, _users, new FakeCurrentUser(currentUserId), _uow);

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsProjectWithCurrentUserAsOwnerAndNoMembers()
    {
        var command = new CreateProjectCommand("Tablero");

        var result = await CreateHandler(currentUserId: 1).HandleAsync(command, CancellationToken.None);

        Assert.Equal(1, result.Id);
        Assert.Equal("Tablero", result.Name);
        Assert.Equal(1, result.Owner.Id);
        Assert.Equal("Ana Gómez", result.Owner.Name);
        Assert.Empty(result.Members);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_AddsProjectOwnedByCurrentUserAndSavesOnce()
    {
        var command = new CreateProjectCommand("Tablero");

        await CreateHandler(currentUserId: 1).HandleAsync(command, CancellationToken.None);

        var project = Assert.Single(_projects.Projects);
        Assert.Equal(ProjectRole.Owner, project.RoleOf(1));
        Assert.Equal(1, _uow.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_NoCurrentUser_ThrowsNotAuthenticatedWithoutSaving()
    {
        var command = new CreateProjectCommand("Tablero");

        await Assert.ThrowsAsync<NotAuthenticatedException>(
            () => CreateHandler(currentUserId: null).HandleAsync(command, CancellationToken.None));
        Assert.Empty(_projects.Projects);
        Assert.Equal(0, _uow.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_UserNoLongerExists_ThrowsNotAuthenticatedWithoutSaving()
    {
        var command = new CreateProjectCommand("Tablero");

        await Assert.ThrowsAsync<NotAuthenticatedException>(
            () => CreateHandler(currentUserId: 99).HandleAsync(command, CancellationToken.None));
        Assert.Empty(_projects.Projects);
        Assert.Equal(0, _uow.SaveChangesCalls);
    }
}
