using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Application.Users;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Projects.AddMember;

public class AddProjectMemberHandler : ICommandHandler<AddProjectMemberCommand>
{
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _uow;

    public AddProjectMemberHandler(IProjectRepository projects, IUserRepository users, ICurrentUser currentUser, IUnitOfWork uow)
    {
        _projects = projects;
        _users = users;
        _currentUser = currentUser;
        _uow = uow;
    }

    // Load, then authorize (ADR 0009). The user lookup comes after authorization on purpose: before it,
    // anyone could tell which user ids exist by comparing 400 and 403 on a project that is not theirs.
    public async Task HandleAsync(AddProjectMemberCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new NotAuthenticatedException();
        var project = await _projects.GetByIdWithMembersAsync(command.ProjectId, cancellationToken)
            ?? throw ProjectErrors.NotFound();

        if (!project.Can(userId, ProjectPermission.ManageMembers))
            throw ProjectErrors.Forbidden();

        if (await _users.GetByIdAsync(command.UserId, cancellationToken) is null)
            throw ProjectErrors.UserToAddNotFound();

        project.AddMember(command.UserId);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
