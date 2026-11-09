using System.Collections.Frozen;

namespace TaskTracker.Domain.Enums;

public static class ProjectRoleExtensions
{
    // Role → permissions map (ADR 0009). A new role or permission only changes this table.
    private static readonly FrozenDictionary<ProjectRole, FrozenSet<ProjectPermission>> Permissions =
        new Dictionary<ProjectRole, FrozenSet<ProjectPermission>>
        {
            [ProjectRole.Owner] = Enum.GetValues<ProjectPermission>().ToFrozenSet(),
            [ProjectRole.Member] = new[] { ProjectPermission.ViewProject }.ToFrozenSet(),
        }.ToFrozenDictionary();

    public static bool Grants(this ProjectRole role, ProjectPermission permission) =>
        Permissions[role].Contains(permission);
}
