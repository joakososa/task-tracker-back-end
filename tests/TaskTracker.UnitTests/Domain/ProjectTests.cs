using TaskTracker.Domain.Common;
using TaskTracker.Domain.Entities;

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

        Assert.Equal(DomainErrors.FieldRequired, ex.Code);
    }

    [Fact]
    public void Constructor_WithNameOverMaxLength_ThrowsFieldTooLong()
    {
        var name = new string('a', Project.NameMaxLength + 1);

        var ex = Assert.Throws<DomainException>(() => new Project(name, 1));

        Assert.Equal(DomainErrors.FieldTooLong, ex.Code);
    }

    [Fact]
    public void Remame_WithSpaceAtEnd_TrimsName()
    {
        var project = CreateProjectWithOwner(1);

        project.Rename("Project Name   ");

        Assert.Equal("Project Name", project.Name);
    }

    [Fact]
    public void IsMember_IsOwner_ReturnsFalse()
    {
        var userId = 2;
        var project = CreateProjectWithOwner(userId);

        var result = project.IsMember(userId);


        Assert.False(result);
    }

    [Fact]
    public void HasAccess_ForMember_ReturnsTrue()
    {
        var userId = 2;
        var project = CreateProjectWithOwner(1);
        project.AddMember(userId);

        var result = project.HasAccess(userId);

        Assert.True(result);
    }

    [Fact]
    public void HasAccess_ForOwner_ReturnsTrue()
    {
        var userId = 2;
        var project = CreateProjectWithOwner(userId);

        var result = project.HasAccess(userId);


        Assert.True(result);
    }


    [Fact]
    public void HasAccess_ForStranger_ReturnsFalse()
    {
        var userId = 2;
        var project = CreateProjectWithOwner(1);

        var result = project.HasAccess(userId);


        Assert.False(result);
    }

    [Fact]
    public void AddMember_IsOwner_Throws()
    {
        var userId = 2;
        var project = CreateProjectWithOwner(userId);

        var ex = Assert.Throws<DomainException>(() => project.AddMember(userId));

        Assert.Equal(DomainErrors.Project.InvalidMember, ex.Code);
        Assert.False(project.IsMember(userId));
    }

    [Fact]
    public void AddMember_IsMember_Throws()
    {
        var project = CreateProjectWithOwner(1);
        var userId = 2;
        project.AddMember(userId);

        var ex = Assert.Throws<DomainException>(() => project.AddMember(userId));

        Assert.Equal(DomainErrors.Project.InvalidMember, ex.Code);
        Assert.Single(project.Members);
    }

    [Fact]
    public void AddMember_NewUser_AddsMemberWithJoinedAt()
    {
        var project = CreateProjectWithOwner(1);
        var before = DateTimeOffset.UtcNow;

        project.AddMember(2);

        var member = Assert.Single(project.Members);
        Assert.Equal(2, member.UserId);
        Assert.True(project.IsMember(2));
        Assert.InRange(member.JoinedAt, before, DateTimeOffset.UtcNow);
    }

    [Fact]
    public void AddMember_SeveralUsers_KeepsAllOfThem()
    {
        var project = CreateProjectWithOwner(1);

        project.AddMember(2);
        project.AddMember(3);

        Assert.Equal(2, project.Members.Count);
        Assert.True(project.HasAccess(2));
        Assert.True(project.HasAccess(3));
    }
}
