using FluentValidation;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Projects.Create;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(Project.NameMaxLength);
    }
}
