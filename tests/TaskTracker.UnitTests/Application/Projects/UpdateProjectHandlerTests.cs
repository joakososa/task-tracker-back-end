using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Projects.Update;
using TaskTracker.Domain.Entities;
using TaskTracker.UnitTests.Application.Fakes;
using TaskTracker.UnitTests.Fakes;

namespace TaskTracker.UnitTests.Application.Projects;

public class UpdateProjectHandlerTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeProjectRepository _projects;
    private readonly FakeUnitOfWork _uow = new();

    // Ana (1) owns project 1, Bea (2) is a member, Carla (3) is a stranger.
    public UpdateProjectHandlerTests()
    {
        _projects = new FakeProjectRepository(_users);
        _users.Add(new User("Ana", "ana@example.com", "securepassword", new FakePasswordHasher()));
        _users.Add(new User("Bea", "bea@example.com", "securepassword", new FakePasswordHasher()));
        _users.Add(new User("Carla", "carla@example.com", "securepassword", new FakePasswordHasher()));
        var project = new Project("Tablero", 1);
        project.AddMember(2);
        _projects.Add(project);
    }

    private UpdateProjectHandler CreateHandler(int? currentUserId) =>
        new(_projects, new FakeCurrentUser(currentUserId), _uow);

    [Fact]
    public async Task HandleAsync_Owner_ReturnsRenamedProject()
    {
        var result = await CreateHandler(currentUserId: 1)
            .HandleAsync(new UpdateProjectCommand(1, "Tablero nuevo"), CancellationToken.None);

        Assert.Equal(1, result.Id);
        Assert.Equal("Tablero nuevo", result.Name);
    }

    [Fact]
    public async Task HandleAsync_Owner_SavesChanges()
    {
        await CreateHandler(currentUserId: 1)
            .HandleAsync(new UpdateProjectCommand(1, "Tablero nuevo"), CancellationToken.None);

        Assert.Equal(1, _uow.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_Member_ThrowsForbiddenAndKeepsName()
    {
        var ex = await Assert.ThrowsAsync<ForbiddenException>(
            () => CreateHandler(currentUserId: 2).HandleAsync(new UpdateProjectCommand(1, "Tablero nuevo"), CancellationToken.None));

        Assert.Equal("PROJECT_FORBIDDEN", ex.Code);
        Assert.Equal("Tablero", _projects.Projects[0].Name);
        Assert.Equal(0, _uow.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_Stranger_ThrowsForbiddenWithProjectCode()
    {
        var ex = await Assert.ThrowsAsync<ForbiddenException>(
            () => CreateHandler(currentUserId: 3).HandleAsync(new UpdateProjectCommand(1, "Tablero nuevo"), CancellationToken.None));

        Assert.Equal("PROJECT_FORBIDDEN", ex.Code);
    }

    [Fact]
    public async Task HandleAsync_MissingProject_ThrowsNotFoundWithProjectCode()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateHandler(currentUserId: 1).HandleAsync(new UpdateProjectCommand(99, "Tablero nuevo"), CancellationToken.None));

        Assert.Equal("PROJECT_NOT_FOUND", ex.Code);
    }

    [Fact]
    public async Task HandleAsync_NoCurrentUser_ThrowsNotAuthenticated()
    {
        await Assert.ThrowsAsync<NotAuthenticatedException>(
            () => CreateHandler(currentUserId: null).HandleAsync(new UpdateProjectCommand(1, "Tablero nuevo"), CancellationToken.None));
    }
}
