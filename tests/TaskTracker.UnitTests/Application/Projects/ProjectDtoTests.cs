using TaskTracker.Application.Projects;
using TaskTracker.Domain.Entities;
using TaskTracker.UnitTests.Application.Fakes;
using TaskTracker.UnitTests.Fakes;

namespace TaskTracker.UnitTests.Application.Projects;

public class ProjectDtoTests
{
    private readonly FakeUserRepository _users = new();

    public ProjectDtoTests()
    {
        _users.Add(new User("Ana", "ana@example.com", "securepassword", new FakePasswordHasher()));
        _users.Add(new User("Bea", "bea@example.com", "securepassword", new FakePasswordHasher()));
        _users.Add(new User("Carla", "carla@example.com", "securepassword", new FakePasswordHasher()));
    }

    private Project CreateProjectOwnedByAnaWithMembers(params int[] memberIds)
    {
        var project = new Project("Tablero", 1);
        foreach (var id in memberIds)
            project.AddMember(id);
        FakeProjectRepository.LoadMemberUsers(project, _users.Users);
        return project;
    }

    [Fact]
    public void FromEntity_ProjectWithMembers_ReturnsOwnerSeparately()
    {
        var project = CreateProjectOwnedByAnaWithMembers(2, 3);

        var dto = ProjectDto.FromEntity(project);

        Assert.Equal(1, dto.Owner.Id);
        Assert.Equal("Ana", dto.Owner.Name);
    }

    [Fact]
    public void FromEntity_ProjectWithMembers_ReturnsOnlyMemberRoleInMembers()
    {
        var project = CreateProjectOwnedByAnaWithMembers(2, 3);

        var dto = ProjectDto.FromEntity(project);

        Assert.Equal([2, 3], dto.Members.Select(m => m.Id).Order());
    }

    [Fact]
    public void FromEntity_ProjectWithOnlyOwner_ReturnsNoMembers()
    {
        var project = CreateProjectOwnedByAnaWithMembers();

        var dto = ProjectDto.FromEntity(project);

        Assert.Empty(dto.Members);
    }
}
