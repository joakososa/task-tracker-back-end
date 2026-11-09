using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Projects;

public interface IProjectRepository
{
    void Add(Project project);
}
