using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Projects.GetById;

public class GetProjectHandler : IQueryHandler<GetProjectQuery, ProjectDto>
{
    private readonly IProjectRepository _projects;
    private readonly ICurrentUser _currentUser;

    public GetProjectHandler(IProjectRepository projects, ICurrentUser currentUser)
    {
        _projects = projects;
        _currentUser = currentUser;
    }

    // Load, then authorize (ADR 0009): 404 when it does not exist, 403 when it exists but is not yours.
    public async Task<ProjectDto> HandleAsync(GetProjectQuery query, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new NotAuthenticatedException();
        var project = await _projects.GetByIdWithMembersAsync(query.ProjectId, cancellationToken)
            ?? throw ProjectErrors.NotFound();

        if (!project.Can(userId, ProjectPermission.ViewProject))
            throw ProjectErrors.Forbidden();

        return ProjectDto.FromEntity(project);
    }
}
