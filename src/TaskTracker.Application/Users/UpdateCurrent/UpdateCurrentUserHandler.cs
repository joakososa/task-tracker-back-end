using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Common.Interfaces;

namespace TaskTracker.Application.Users.UpdateCurrent;

public class UpdateCurrentUserHandler : ICommandHandler<UpdateCurrentUserCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _uow;

    public UpdateCurrentUserHandler(IUserRepository users, ICurrentUser currentUser, IUnitOfWork uow)
    {
        _users = users;
        _currentUser = currentUser;
        _uow = uow;
    }

    public async Task<UserDto> HandleAsync(UpdateCurrentUserCommand command, CancellationToken cancellationToken)
    {
        // A valid token can outlive its user, so a missing user is treated like a missing token.
        var userId = _currentUser.UserId ?? throw new NotAuthenticatedException();
        var user = await _users.GetByIdAsync(userId, cancellationToken) ?? throw new NotAuthenticatedException();

        user.UpdateProfile(command.Name, command.Avatar);
        await _uow.SaveChangesAsync(cancellationToken);

        return UserDto.FromEntity(user);
    }
}
