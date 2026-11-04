using FluentValidation;

namespace TaskTracker.Application.Auth.Login;

/// Only checks that both fields are present. The password policy is a registration rule;
/// a login just checks whether the password matches.
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty();
        RuleFor(command => command.Password).NotEmpty();
    }
}
