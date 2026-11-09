using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Projects.AddMember;
using TaskTracker.Domain.Common;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.UnitTests.Application.Fakes;
using TaskTracker.UnitTests.Fakes;

namespace TaskTracker.UnitTests.Application.Projects;

public class AddProjectMemberHandlerTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeProjectRepository _projects;
    private readonly FakeUnitOfWork _uow = new();

    // Ana (1) owns project 1, Bea (2) is a member, Carla (3) is not in it yet.
    public AddProjectMemberHandlerTests()
    {
        _projects = new FakeProjectRepository(_users);
        _users.Add(new User("Ana", "ana@example.com", "securepassword", new FakePasswordHasher()));
        _users.Add(new User("Bea", "bea@example.com", "securepassword", new FakePasswordHasher()));
        _users.Add(new User("Carla", "carla@example.com", "securepassword", new FakePasswordHasher()));
        var project = new Project("Tablero", 1);
        project.AddMember(2);
        _projects.Add(project);
    }

    private AddProjectMemberHandler CreateHandler(int? currentUserId) =>
        new(_projects, _users, new FakeCurrentUser(currentUserId), _uow);

    [Fact]
    public async Task HandleAsync_OwnerAddsNewUser_UserBecomesMember()
    {
        await CreateHandler(currentUserId: 1).HandleAsync(new AddProjectMemberCommand(1, 3), CancellationToken.None);

        Assert.Equal(ProjectRole.Member, _projects.Projects[0].RoleOf(3));
        Assert.Equal(1, _uow.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_UserAlreadyMember_ThrowsMemberAlreadyExists()
    {
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => CreateHandler(currentUserId: 1).HandleAsync(new AddProjectMemberCommand(1, 2), CancellationToken.None));

        Assert.Equal("PROJECT_MEMBER_ALREADY_EXISTS", ex.Code);
    }

    [Fact]
    public async Task HandleAsync_UserIsOwner_ThrowsMemberAlreadyExists()
    {
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => CreateHandler(currentUserId: 1).HandleAsync(new AddProjectMemberCommand(1, 1), CancellationToken.None));

        Assert.Equal("PROJECT_MEMBER_ALREADY_EXISTS", ex.Code);
    }

    [Fact]
    public async Task HandleAsync_Member_ThrowsForbiddenAndAddsNobody()
    {
        var ex = await Assert.ThrowsAsync<ForbiddenException>(
            () => CreateHandler(currentUserId: 2).HandleAsync(new AddProjectMemberCommand(1, 3), CancellationToken.None));

        Assert.Equal("PROJECT_FORBIDDEN", ex.Code);
        Assert.Null(_projects.Projects[0].RoleOf(3));
        Assert.Equal(0, _uow.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_MissingProject_ThrowsNotFoundWithProjectCode()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateHandler(currentUserId: 1).HandleAsync(new AddProjectMemberCommand(99, 3), CancellationToken.None));

        Assert.Equal("PROJECT_NOT_FOUND", ex.Code);
    }

    [Fact]
    public async Task HandleAsync_MissingUser_ThrowsInvalidRequestWithUserNotFoundCode()
    {
        var ex = await Assert.ThrowsAsync<InvalidRequestException>(
            () => CreateHandler(currentUserId: 1).HandleAsync(new AddProjectMemberCommand(1, 99), CancellationToken.None));

        Assert.Equal("USER_NOT_FOUND", ex.Code);
        Assert.Equal(0, _uow.SaveChangesCalls);
    }

    [Fact]
    public async Task HandleAsync_NoCurrentUser_ThrowsNotAuthenticated()
    {
        await Assert.ThrowsAsync<NotAuthenticatedException>(
            () => CreateHandler(currentUserId: null).HandleAsync(new AddProjectMemberCommand(1, 3), CancellationToken.None));
    }
}
