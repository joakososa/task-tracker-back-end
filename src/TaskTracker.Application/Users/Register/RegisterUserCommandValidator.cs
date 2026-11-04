using FluentValidation;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Users.Register;

/// Validates the whole command so the client gets every error at once.
/// Limits come from the Domain constants; the entity re-checks them as a safety net.
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(User.NameMaxLength);

        RuleFor(command => command.Email)
            .NotEmpty()
            .MaximumLength(User.EmailMaxLength)
            .EmailAddress();

        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(User.PasswordMinLength)
            .MaximumLength(User.PasswordMaxLength);

        RuleFor(command => command.Avatar)
            .MaximumLength(User.AvatarUrlMaxLength);
    }
}
