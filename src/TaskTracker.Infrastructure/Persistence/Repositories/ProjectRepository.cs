using TaskTracker.Application.Projects;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Infrastructure.Persistence.Repositories;

public class ProjectRepository(AppDbContext db) : IProjectRepository
{
    public void Add(Project project) => db.Projects.Add(project);
}
