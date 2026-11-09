using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Projects.List;
using TaskTracker.Domain.Entities;
using TaskTracker.UnitTests.Application.Fakes;
using TaskTracker.UnitTests.Fakes;

namespace TaskTracker.UnitTests.Application.Projects;

// Which projects a user sees is decided by the repository's query; HTTP tests cover it against SQL Server.
public class ListProjectsHandlerTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeProjectRepository _projects;

    public ListProjectsHandlerTests()
    {
        _projects = new FakeProjectRepository(_users);
        _users.Add(new User("Ana", "ana@example.com", "securepassword", new FakePasswordHasher()));
        _projects.Add(new Project("Tablero", 1));
        _projects.Add(new Project("Backlog", 1));
    }

    private ListProjectsHandler CreateHandler(int? currentUserId) =>
        new(_projects, new FakeCurrentUser(currentUserId));

    [Fact]
    public async Task HandleAsync_UserWithProjects_ReturnsThemMapped()
    {
        var result = await CreateHandler(currentUserId: 1).HandleAsync(new ListProjectsQuery(), CancellationToken.None);

        Assert.Equal(["Tablero", "Backlog"], result.Select(p => p.Name));
        Assert.All(result, p => Assert.Equal(1, p.Owner.Id));
    }

    [Fact]
    public async Task HandleAsync_NoCurrentUser_ThrowsNotAuthenticated()
    {
        await Assert.ThrowsAsync<NotAuthenticatedException>(
            () => CreateHandler(currentUserId: null).HandleAsync(new ListProjectsQuery(), CancellationToken.None));
    }
}
