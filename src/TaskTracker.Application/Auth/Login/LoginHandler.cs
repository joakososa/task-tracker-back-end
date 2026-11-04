using TaskTracker.Application.Common.Exceptions;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Application.Users;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Services;

namespace TaskTracker.Application.Auth.Login;

public class LoginHandler : ICommandHandler<LoginCommand, AccessTokenDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public LoginHandler(IUserRepository users, IPasswordHasher hasher, ITokenService tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<AccessTokenDto> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(User.NormalizeEmail(command.Email), cancellationToken);

        // An unknown email returns faster than a wrong password (no hash to verify), which leaks which emails exist.
        // Not equalized on purpose: POST /users already reveals registered emails with a 409.
        if (user is null || !user.VerifyPassword(command.Password, _hasher))
        {
            throw new AuthenticationFailedException();
        }

        return _tokens.CreateAccessToken(user);
    }
}
