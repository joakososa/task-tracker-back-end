using FluentValidation.TestHelper;
using TaskTracker.Application.Projects.Create;

namespace TaskTracker.UnitTests.Application.Projects;

public class CreateProjectCommandValidatorTests
{
    private readonly CreateProjectCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankName_HasOnlyNameError(string name)
    {
        var command = new CreateProjectCommand(name);

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(c => c.Name)
            .Only();
    }

    [Fact]
    public void Validate_NameLongerThan200_HasOnlyNameError()
    {
        var command = new CreateProjectCommand(new string('A', 201));

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(c => c.Name)
            .Only();
    }

    [Fact]
    public void Validate_NameOf200_HasNoErrors()
    {
        var command = new CreateProjectCommand(new string('A', 200));

        _validator.TestValidate(command)
            .ShouldNotHaveAnyValidationErrors();
    }
}
