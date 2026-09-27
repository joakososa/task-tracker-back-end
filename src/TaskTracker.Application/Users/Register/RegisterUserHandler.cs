using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Domain.Services;
using TaskTracker.Domain.Common;
using TaskTracker.Domain.Entities;
namespace TaskTracker.Application.Users.Register;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand,UserDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IUnitOfWork _uow;

    public RegisterUserHandler(IUserRepository users, IPasswordHasher hasher, IUnitOfWork uow)
    {
        _users = users;
        _hasher = hasher;
        _uow = uow;
    }

    public async Task<UserDto> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken) 
    {
        if (await _users.ExistsByEmailAsync(User.NormalizeEmail(command.Email), cancellationToken))
        {
            throw new DomainException(DomainErrors.User.EmailAlreadyExists, "The email is already registered.");
        }

        var user = new User(command.Name, command.Email, command.Password, _hasher, command.Avatar);

        _users.Add(user);
        await _uow.SaveChangesAsync(cancellationToken);
        return UserDto.FromEntity(user);
    }
}