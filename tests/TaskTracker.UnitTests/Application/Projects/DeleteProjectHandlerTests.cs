using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Projects.Delete;
using TaskTracker.Domain.Entities;
using TaskTracker.UnitTests.Application.Fakes;
using TaskTracker.UnitTests.Fakes;

namespace TaskTracker.UnitTests.Application.Projects;

public class DeleteProjectHandlerTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeProjectRepository _projects;
    private readonly FakeUnitOfWork _uow = new();

    // Ana (1) owns project 1, Bea (2) is a member.
    public DeleteProjectHandlerTests()
    {
        _projects = new FakeProjectRepository(_users);
        _users.Add(new User("Ana", "ana@example.com", "securepassword", new FakePasswordHasher()));
        _users.Add(new User("Bea", "bea@example.com", "securepassword", new FakePasswordHasher()));
        var project = new Project("Tablero", 1);
        project.AddMember(2);
        _projects.Add(project);
    }

    private DeleteProjectHandler CreateHandler(int? currentUserId) =>
        new(_projects, new FakeCurrentUser(currentUserId), _uow);

    [Fact]
    public async Task HandleAsync_Owner_RemovesProjectAndSaves()
    {
        await CreateHandler(currentUserId: 1).HandleAsync(new DeleteProjectCommand(1), CancellationToken.None);

        Assert.Empty(_projects.Projects);
        Assert.Equal(1, _uow.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_Member_ThrowsForbiddenAndKeepsProject()
    {
        var ex = await Assert.ThrowsAsync<ForbiddenException>(
            () => CreateHandler(currentUserId: 2).HandleAsync(new DeleteProjectCommand(1), CancellationToken.None));

        Assert.Equal("PROJECT_FORBIDDEN", ex.Code);
        Assert.Single(_projects.Projects);
        Assert.Equal(0, _uow.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_MissingProject_ThrowsNotFoundWithProjectCode()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateHandler(currentUserId: 1).HandleAsync(new DeleteProjectCommand(99), CancellationToken.None));

        Assert.Equal("PROJECT_NOT_FOUND", ex.Code);
    }

    [Fact]
    public async Task HandleAsync_NoCurrentUser_ThrowsNotAuthenticated()
    {
        await Assert.ThrowsAsync<NotAuthenticatedException>(
            () => CreateHandler(currentUserId: null).HandleAsync(new DeleteProjectCommand(1), CancellationToken.None));
    }
}
