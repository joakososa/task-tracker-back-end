using FluentValidation.TestHelper;
using TaskTracker.Application.Projects.AddMember;

namespace TaskTracker.UnitTests.Application.Projects;

public class AddProjectMemberCommandValidatorTests
{
    private readonly AddProjectMemberCommandValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_UserIdNotPositive_HasOnlyUserIdError(int userId)
    {
        var command = new AddProjectMemberCommand(1, userId);

        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(c => c.UserId)
            .Only();
    }

    [Fact]
    public void Validate_UserIdOf1_HasNoErrors()
    {
        var command = new AddProjectMemberCommand(1, 1);

        _validator.TestValidate(command)
            .ShouldNotHaveAnyValidationErrors();
    }
}
