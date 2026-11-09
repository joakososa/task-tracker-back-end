using TaskTracker.Application.Projects;
using TaskTracker.Domain.Entities;

namespace TaskTracker.UnitTests.Application.Fakes;

/// Reads behave like the EF repository's Include: each member gets its User from <paramref name="users"/>.
public class FakeProjectRepository(FakeUserRepository users) : IProjectRepository
{
    private readonly List<Project> _projects = [];
    public IReadOnlyList<Project> Projects => _projects;

    // Assigns ids 1, 2, 3... like the database identity column, through the private setter as EF does.
    public void Add(Project project)
    {
        typeof(Project).GetProperty(nameof(Project.Id))!.SetValue(project, _projects.Count + 1);
        _projects.Add(project);
    }

    public void Remove(Project project) => _projects.Remove(project);

    public Task<Project?> GetByIdWithMembersAsync(int id, CancellationToken cancellationToken)
    {
        var project = _projects.SingleOrDefault(p => p.Id == id);
        if (project is not null)
            LoadMemberUsers(project, users.Users);
        return Task.FromResult(project);
    }

    public Task<IReadOnlyList<Project>> ListForUserAsync(int userId, CancellationToken cancellationToken)
    {
        IReadOnlyList<Project> result = _projects.Where(p => p.RoleOf(userId) is not null).OrderBy(p => p.Id).ToList();
        foreach (var project in result)
            LoadMemberUsers(project, users.Users);
        return Task.FromResult(result);
    }

    /// What EF's ThenInclude(m => m.User) does: sets each member's navigation through its private setter.
    public static void LoadMemberUsers(Project project, IEnumerable<User> users)
    {
        foreach (var member in project.Members)
        {
            var user = users.Single(u => u.Id == member.UserId);
            typeof(ProjectMember).GetProperty(nameof(ProjectMember.User))!.SetValue(member, user);
        }
    }
}
