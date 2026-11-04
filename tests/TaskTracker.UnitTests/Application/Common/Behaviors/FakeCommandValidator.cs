using FluentValidation;
namespace TaskTracker.UnitTests.Application.Fakes;

public class FakeCommandValidator : AbstractValidator<FakeCommand>
{
    public FakeCommandValidator()
    {
        RuleFor(command => command.Property1)
            .NotNull()
            .NotEmpty()
            .EmailAddress().WithMessage("Tira error bien");
    }
}