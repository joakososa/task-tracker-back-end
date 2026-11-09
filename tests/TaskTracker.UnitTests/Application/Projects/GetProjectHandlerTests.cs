using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Projects.GetById;
using TaskTracker.Domain.Entities;
using TaskTracker.UnitTests.Application.Fakes;
using TaskTracker.UnitTests.Fakes;

namespace TaskTracker.UnitTests.Application.Projects;

public class GetProjectHandlerTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeProjectRepository _projects;

    // Ana (1) owns project 1, Bea (2) is a member, Carla (3) is a stranger.
    public GetProjectHandlerTests()
    {
        _projects = new FakeProjectRepository(_users);
        _users.Add(new User("Ana", "ana@example.com", "securepassword", new FakePasswordHasher()));
        _users.Add(new User("Bea", "bea@example.com", "securepassword", new FakePasswordHasher()));
        _users.Add(new User("Carla", "carla@example.com", "securepassword", new FakePasswordHasher()));
        var project = new Project("Tablero", 1);
        project.AddMember(2);
        _projects.Add(project);
    }

    private GetProjectHandler CreateHandler(int? currentUserId) =>
        new(_projects, new FakeCurrentUser(currentUserId));

    [Fact]
    public async Task HandleAsync_Owner_ReturnsProject()
    {
        var result = await CreateHandler(currentUserId: 1).HandleAsync(new GetProjectQuery(1), CancellationToken.None);

        Assert.Equal(1, result.Id);
        Assert.Equal("Tablero", result.Name);
    }

    [Fact]
    public async Task HandleAsync_Member_ReturnsProject()
    {
        var result = await CreateHandler(currentUserId: 2).HandleAsync(new GetProjectQuery(1), CancellationToken.None);

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task HandleAsync_Stranger_ThrowsForbiddenWithProjectCode()
    {
        var ex = await Assert.ThrowsAsync<ForbiddenException>(
            () => CreateHandler(currentUserId: 3).HandleAsync(new GetProjectQuery(1), CancellationToken.None));

        Assert.Equal("PROJECT_FORBIDDEN", ex.Code);
    }

    [Fact]
    public async Task HandleAsync_MissingProject_ThrowsNotFoundWithProjectCode()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateHandler(currentUserId: 1).HandleAsync(new GetProjectQuery(99), CancellationToken.None));

        Assert.Equal("PROJECT_NOT_FOUND", ex.Code);
    }

    [Fact]
    public async Task HandleAsync_NoCurrentUser_ThrowsNotAuthenticated()
    {
        await Assert.ThrowsAsync<NotAuthenticatedException>(
            () => CreateHandler(currentUserId: null).HandleAsync(new GetProjectQuery(1), CancellationToken.None));
    }
}
