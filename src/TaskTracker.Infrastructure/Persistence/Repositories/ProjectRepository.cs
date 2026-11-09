using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.Projects;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Infrastructure.Persistence.Repositories;

public class ProjectRepository(AppDbContext db) : IProjectRepository
{
    public void Add(Project project) => db.Projects.Add(project);

    // Tracked: the update, delete and add-member use cases change what this returns.
    public void Remove(Project project) => db.Projects.Remove(project);

    public Task<Project?> GetByIdWithMembersAsync(int id, CancellationToken cancellationToken) =>
        db.Projects
            .Include(p => p.Members).ThenInclude(m => m.User)
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

    // No tracking: read-only, and it can return many projects. One collection plus a reference,
    // so a single query has no cartesian explosion (no AsSplitQuery needed).
    public async Task<IReadOnlyList<Project>> ListForUserAsync(int userId, CancellationToken cancellationToken) =>
        await db.Projects
            .AsNoTracking()
            .Where(p => p.Members.Any(m => m.UserId == userId))
            .Include(p => p.Members).ThenInclude(m => m.User)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
}
