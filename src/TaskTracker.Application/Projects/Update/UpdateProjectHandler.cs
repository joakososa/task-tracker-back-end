using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Projects.Update;

public class UpdateProjectHandler : ICommandHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly IProjectRepository _projects;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _uow;

    public UpdateProjectHandler(IProjectRepository projects, ICurrentUser currentUser, IUnitOfWork uow)
    {
        _projects = projects;
        _currentUser = currentUser;
        _uow = uow;
    }

    // Load, then authorize (ADR 0009): only the owner has EditProject.
    public async Task<ProjectDto> HandleAsync(UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new NotAuthenticatedException();
        var project = await _projects.GetByIdWithMembersAsync(command.ProjectId, cancellationToken)
            ?? throw ProjectErrors.NotFound();

        if (!project.Can(userId, ProjectPermission.EditProject))
            throw ProjectErrors.Forbidden();

        project.Rename(command.Name);
        await _uow.SaveChangesAsync(cancellationToken);

        return ProjectDto.FromEntity(project);
    }
}
