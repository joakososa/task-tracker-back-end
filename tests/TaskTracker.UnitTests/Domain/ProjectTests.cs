using TaskTracker.Domain.Common;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.UnitTests.Domain;

public class ProjectTests
{
    private static Project CreateProjectWithOwner(int ownerId)
    {
        return new Project("Project Name", ownerId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithBlankName_Throws(string name)
    {
        var ex = Assert.Throws<DomainException>(() => new Project(name, 1));

        Assert.Equal("FIELD_REQUIRED", ex.Code);
    }

    [Fact]
    public void Constructor_WithNameOverMaxLength_ThrowsFieldTooLong()
    {
        var name = new string('a', Project.NameMaxLength + 1);

        var ex = Assert.Throws<DomainException>(() => new Project(name, 1));

        Assert.Equal("FIELD_TOO_LONG", ex.Code);
    }

    [Fact]
    public void Constructor_WithOwner_AddsOwnerAsOnlyMember()
    {
        var project = CreateProjectWithOwner(1);

        var member = Assert.Single(project.Members);
        Assert.Equal(1, member.UserId);
        Assert.Equal(ProjectRole.Owner, member.Role);
    }

    [Fact]
    public void Rename_WithSpaceAtEnd_TrimsName()
    {
        var project = CreateProjectWithOwner(1);

        project.Rename("Project Name   ");

        Assert.Equal("Project Name", project.Name);
    }

    [Fact]
    public void RoleOf_Owner_ReturnsOwner()
    {
        var project = CreateProjectWithOwner(1);

        var role = project.RoleOf(1);

        Assert.Equal(ProjectRole.Owner, role);
    }

    [Fact]
    public void RoleOf_AddedMember_ReturnsMember()
    {
        var project = CreateProjectWithOwner(1);
        project.AddMember(2);

        var role = project.RoleOf(2);

        Assert.Equal(ProjectRole.Member, role);
    }

    [Fact]
    public void RoleOf_Stranger_ReturnsNull()
    {
        var project = CreateProjectWithOwner(1);

        var role = project.RoleOf(2);

        Assert.Null(role);
    }

    [Fact]
    public void AddMember_NewUser_AddsMemberWithJoinedAt()
    {
        var project = CreateProjectWithOwner(1);
        var before = DateTimeOffset.UtcNow;

        project.AddMember(2);

        var member = Assert.Single(project.Members, m => m.UserId == 2);
        Assert.Equal(ProjectRole.Member, member.Role);
        Assert.InRange(member.JoinedAt, before, DateTimeOffset.UtcNow);
    }

    [Fact]
    public void AddMember_SeveralUsers_KeepsAllOfThem()
    {
        var project = CreateProjectWithOwner(1);

        project.AddMember(2);
        project.AddMember(3);

        Assert.Equal(3, project.Members.Count);
    }

    [Fact]
    public void AddMember_ExistingMember_ThrowsMemberAlreadyExists()
    {
        var project = CreateProjectWithOwner(1);
        project.AddMember(2);

        var ex = Assert.Throws<DomainException>(() => project.AddMember(2));

        Assert.Equal("PROJECT_MEMBER_ALREADY_EXISTS", ex.Code);
        Assert.Equal(2, project.Members.Count);
    }

    [Fact]
    public void AddMember_Owner_ThrowsMemberAlreadyExists()
    {
        var project = CreateProjectWithOwner(1);

        var ex = Assert.Throws<DomainException>(() => project.AddMember(1));

        Assert.Equal("PROJECT_MEMBER_ALREADY_EXISTS", ex.Code);
        Assert.Equal(ProjectRole.Owner, project.RoleOf(1));
        Assert.Single(project.Members);
    }

    [Fact]
    public void Can_MemberEditProject_ReturnsFalse()
    {
        var project = CreateProjectWithOwner(1);
        project.AddMember(2);

        var result = project.Can(2, ProjectPermission.EditProject);

        Assert.False(result);
    }

    [Fact]
    public void Can_StrangerViewProject_ReturnsFalse()
    {
        var project = CreateProjectWithOwner(1);

        var result = project.Can(2, ProjectPermission.ViewProject);

        Assert.False(result);
    }
}
