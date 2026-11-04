using FluentValidation.TestHelper;
using TaskTracker.Application.Users.UpdateCurrent;

namespace TaskTracker.UnitTests.Application.Users;

public class UpdateCurrentUserCommandValidatorTests
{
    private readonly UpdateCurrentUserCommandValidator _validator = new();

    [Fact]
    public void Validate_EmptyName_HasOnlyNameError()
    {
        var command = new UpdateCurrentUserCommand("", null);

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(c => c.Name)
            .Only();
    }

    [Fact]
    public void Validate_NameLongerThan100_HasOnlyNameError()
    {
        var command = new UpdateCurrentUserCommand(new string('A', 101), null);

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(c => c.Name)
            .Only();
    }

    [Fact]
    public void Validate_AvatarLongerThan2048_HasOnlyAvatarError()
    {
        var command = new UpdateCurrentUserCommand("Ana Gómez", new string('a', 2049));

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(c => c.Avatar)
            .Only();
    }
}
