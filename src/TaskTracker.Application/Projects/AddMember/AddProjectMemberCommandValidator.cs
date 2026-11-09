using FluentValidation;

namespace TaskTracker.Application.Projects.AddMember;

public class AddProjectMemberCommandValidator : AbstractValidator<AddProjectMemberCommand>
{
    // A missing userId binds as 0, so this also covers a body without it.
    // Whether the user exists is checked by the handler, after authorization (see AddProjectMemberHandler).
    public AddProjectMemberCommandValidator()
    {
        RuleFor(command => command.UserId)
            .GreaterThan(0);
    }
}
