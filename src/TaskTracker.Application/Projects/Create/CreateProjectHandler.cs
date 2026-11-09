using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Application.Users;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Projects.Create;

public class CreateProjectHandler : ICommandHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _uow;

    public CreateProjectHandler(IProjectRepository projects, IUserRepository users, ICurrentUser currentUser, IUnitOfWork uow)
    {
        _projects = projects;
        _users = users;
        _currentUser = currentUser;
        _uow = uow;
    }

    public async Task<ProjectDto> HandleAsync(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        // A valid token can outlive its user; without this check the owner foreign key would fail with a 500.
        var userId = _currentUser.UserId ?? throw new NotAuthenticatedException();
        var owner = await _users.GetByIdAsync(userId, cancellationToken) ?? throw new NotAuthenticatedException();

        var project = new Project(command.Name, owner.Id);
        _projects.Add(project);
        await _uow.SaveChangesAsync(cancellationToken);

        // A new project has only its owner, so there are no members to map.
        return new ProjectDto(
            project.Id,
            project.Name,
            UserDto.FromEntity(owner),
            [],
            project.CreatedAt,
            project.UpdatedAt,
            project.CreatedBy);
    }
}
