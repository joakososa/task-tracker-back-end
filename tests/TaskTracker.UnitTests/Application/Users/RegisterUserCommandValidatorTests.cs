using FluentValidation.TestHelper;
using TaskTracker.Application.Users.Register;
namespace TaskTracker.UnitTests.Application.Users;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void Validate_PasswordShorterThan8_HasOnlyPasswordError()
    {
        var command = new RegisterUserCommand("Juan Pérez", "user@example.com", "1234567", null);

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(c => c.Password)
            .Only();
    }

    [Fact]
    public void Validate_PasswordLongerThan128_HasOnlyPasswordError()
    {
        var command = new RegisterUserCommand("Juan Pérez", "user@example.com", new string('A', 129), null);

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(c => c.Password)
            .Only();
    }

    [Fact]
    public void Validate_EmailWithoutAtSign_HasOnlyEmailError()
    {
        var command = new RegisterUserCommand("Juan Pérez", "userexample.com", "12345678", null);

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(c => c.Email)
            .Only();
    }


    [Fact]
    public void Validate_PasswordWithExactly8_HasNoErrors()
    {
        var command = new RegisterUserCommand("Juan Pérez", "user@example.com", "12345678", null);

        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_HasOnlyNameError()
    {
        var command = new RegisterUserCommand("", "user@example.com", "12345678", null);

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(c => c.Name)
            .Only();
    }

    [Fact]
    public void Validate_NameLongerThan100_HasOnlyNameError()
    {
        var command = new RegisterUserCommand(new string('a', 101), "user@example.com", "12345678", null);

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(c => c.Name)
            .Only();
    }
}
