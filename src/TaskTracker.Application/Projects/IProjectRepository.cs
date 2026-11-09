using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Projects;

public interface IProjectRepository
{
    void Add(Project project);

    /// Deletes the project; its members, tasks and comments go with it by cascade.
    void Remove(Project project);

    /// Loads the members and their users: authorization (Project.RoleOf) and ProjectDto need both. Tracked.
    Task<Project?> GetByIdWithMembersAsync(int id, CancellationToken cancellationToken);

    /// Projects where the user has any role, with members and their users, ordered by id. Read-only.
    Task<IReadOnlyList<Project>> ListForUserAsync(int userId, CancellationToken cancellationToken);
}
