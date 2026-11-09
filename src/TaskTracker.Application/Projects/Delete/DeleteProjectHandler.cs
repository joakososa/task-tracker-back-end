using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Projects.Delete;

public class DeleteProjectHandler : ICommandHandler<DeleteProjectCommand>
{
    private readonly IProjectRepository _projects;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _uow;

    public DeleteProjectHandler(IProjectRepository projects, ICurrentUser currentUser, IUnitOfWork uow)
    {
        _projects = projects;
        _currentUser = currentUser;
        _uow = uow;
    }

    // Load, then authorize (ADR 0009): only the owner has DeleteProject.
    public async Task HandleAsync(DeleteProjectCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new NotAuthenticatedException();
        var project = await _projects.GetByIdWithMembersAsync(command.ProjectId, cancellationToken)
            ?? throw ProjectErrors.NotFound();

        if (!project.Can(userId, ProjectPermission.DeleteProject))
            throw ProjectErrors.Forbidden();

        _projects.Remove(project);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
