using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Common.Interfaces;

namespace TaskTracker.Application.Users.GetCurrent;

public class GetCurrentUserHandler : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IUserRepository _users;
    private readonly ICurrentUser _currentUser;

    public GetCurrentUserHandler(IUserRepository users, ICurrentUser currentUser)
    {
        _users = users;
        _currentUser = currentUser;
    }

    public async Task<UserDto> HandleAsync(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        // A valid token can outlive its user, so a missing user is treated like a missing token.
        var userId = _currentUser.UserId ?? throw new NotAuthenticatedException();
        var user = await _users.GetByIdAsync(userId, cancellationToken) ?? throw new NotAuthenticatedException();

        return UserDto.FromEntity(user);
    }
}
