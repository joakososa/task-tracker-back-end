using FluentValidation;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Users.UpdateCurrent;

public class UpdateCurrentUserCommandValidator : AbstractValidator<UpdateCurrentUserCommand>
{
    public UpdateCurrentUserCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(User.NameMaxLength);

        RuleFor(command => command.Avatar)
            .MaximumLength(User.AvatarUrlMaxLength);
    }
}
