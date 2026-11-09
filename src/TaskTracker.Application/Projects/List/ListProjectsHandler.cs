using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Common.Interfaces;

namespace TaskTracker.Application.Projects.List;

public class ListProjectsHandler : IQueryHandler<ListProjectsQuery, IReadOnlyList<ProjectDto>>
{
    private readonly IProjectRepository _projects;
    private readonly ICurrentUser _currentUser;

    public ListProjectsHandler(IProjectRepository projects, ICurrentUser currentUser)
    {
        _projects = projects;
        _currentUser = currentUser;
    }

    // No permission check: the query only returns projects where the user has a role (RF-06).
    public async Task<IReadOnlyList<ProjectDto>> HandleAsync(ListProjectsQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new NotAuthenticatedException();
        var projects = await _projects.ListForUserAsync(userId, cancellationToken);

        return projects.Select(ProjectDto.FromEntity).ToList();
    }
}
