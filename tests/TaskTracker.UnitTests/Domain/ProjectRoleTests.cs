using TaskTracker.Domain.Enums;

namespace TaskTracker.UnitTests.Domain;

public class ProjectRoleTests
{
    [Theory]
    [InlineData(ProjectRole.Owner, ProjectPermission.ViewProject, true)]
    [InlineData(ProjectRole.Owner, ProjectPermission.EditProject, true)]
    [InlineData(ProjectRole.Owner, ProjectPermission.DeleteProject, true)]
    [InlineData(ProjectRole.Owner, ProjectPermission.ManageMembers, true)]
    [InlineData(ProjectRole.Member, ProjectPermission.ViewProject, true)]
    [InlineData(ProjectRole.Member, ProjectPermission.EditProject, false)]
    [InlineData(ProjectRole.Member, ProjectPermission.DeleteProject, false)]
    [InlineData(ProjectRole.Member, ProjectPermission.ManageMembers, false)]
    public void Grants_RoleAndPermission_ReturnsExpected(ProjectRole role, ProjectPermission permission, bool expected)
    {
        var result = role.Grants(permission);

        Assert.Equal(expected, result);
    }
}
