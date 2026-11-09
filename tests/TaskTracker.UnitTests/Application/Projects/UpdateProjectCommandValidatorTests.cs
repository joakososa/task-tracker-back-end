using FluentValidation.TestHelper;
using TaskTracker.Application.Projects.Update;

namespace TaskTracker.UnitTests.Application.Projects;

public class UpdateProjectCommandValidatorTests
{
    private readonly UpdateProjectCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankName_HasOnlyNameError(string name)
    {
        var command = new UpdateProjectCommand(1, name);

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(c => c.Name)
            .Only();
    }

    [Fact]
    public void Validate_NameLongerThan200_HasOnlyNameError()
    {
        var command = new UpdateProjectCommand(1, new string('A', 201));

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(c => c.Name)
            .Only();
    }

    [Fact]
    public void Validate_NameOf200_HasNoErrors()
    {
        var command = new UpdateProjectCommand(1, new string('A', 200));

        _validator.TestValidate(command)
            .ShouldNotHaveAnyValidationErrors();
    }
}
