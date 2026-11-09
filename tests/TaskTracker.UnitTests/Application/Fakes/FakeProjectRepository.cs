using TaskTracker.Application.Projects;
using TaskTracker.Domain.Entities;

namespace TaskTracker.UnitTests.Application.Fakes;

public class FakeProjectRepository : IProjectRepository
{
    private readonly List<Project> _projects = [];
    public IReadOnlyList<Project> Projects => _projects;

    // Assigns ids 1, 2, 3... like the database identity column, through the private setter as EF does.
    public void Add(Project project)
    {
        typeof(Project).GetProperty(nameof(Project.Id))!.SetValue(project, _projects.Count + 1);
        _projects.Add(project);
    }
}
