using TaskTracker.Application.Common;
using TaskTracker.Application.Users;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Projects;

/// Owner and members are separate (contract ProjectResponse): `Members` holds only the Member role, so nobody
/// appears twice.
public sealed record ProjectDto(
    int Id,
    string Name,
    UserDto Owner,
    IReadOnlyList<UserDto> Members,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int? CreatedBy)
    : AuditedDto(CreatedAt, UpdatedAt, CreatedBy)
{
    /// Needs the members and their users loaded (IProjectRepository read methods do it).
    public static ProjectDto FromEntity(Project project)
    {
        var owner = project.Members.Single(m => m.Role == ProjectRole.Owner);
        var members = project.Members
            .Where(m => m.Role == ProjectRole.Member)
            .OrderBy(m => m.JoinedAt)
            .Select(m => UserDto.FromEntity(m.User))
            .ToList();

        return new ProjectDto(
            project.Id,
            project.Name,
            UserDto.FromEntity(owner.User),
            members,
            project.CreatedAt,
            project.UpdatedAt,
            project.CreatedBy);
    }
}
