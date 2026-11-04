using FluentValidation.TestHelper;
using TaskTracker.Application.Auth.Login;

namespace TaskTracker.UnitTests.Application.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_EmptyEmail_HasOnlyEmailError()
    {
        var command = new LoginCommand("", "securepassword");

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(c => c.Email)
            .Only();
    }

    [Fact]
    public void Validate_EmptyPassword_HasOnlyPasswordError()
    {
        var command = new LoginCommand("user@example.com", "");

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(c => c.Password)
            .Only();
    }
}
