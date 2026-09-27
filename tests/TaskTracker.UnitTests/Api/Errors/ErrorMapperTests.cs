using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TaskTracker.Api.Errors;
using TaskTracker.Domain.Common;

namespace TaskTracker.UnitTests.Api.Errors;

public class ErrorMapperTests
{
    [Fact]
    public void Map_ValidationException_Returns400WithErrorsGroupedByCamelCaseField()
    {
        var exception = new ValidationException(
        [
            new ValidationFailure("Password", "Too short."),
            new ValidationFailure("Password", "Must not be empty."),
            new ValidationFailure("Email", "Invalid email."),
        ]);

        var (status, body) = ErrorMapper.Map(exception);

        Assert.Equal(400, status);
        Assert.Equal("INVALID_INPUT", body.Code);
        Assert.Equal(["Too short.", "Must not be empty."], body.Errors!["password"]);
        Assert.Equal(["Invalid email."], body.Errors["email"]);
    }

    [Theory]
    [InlineData(ErrorKind.Validation, 400)]
    [InlineData(ErrorKind.Conflict, 409)]
    [InlineData(ErrorKind.NotFound, 404)]
    [InlineData(ErrorKind.Forbidden, 403)]
    public void Map_DomainException_ReturnsStatusForItsKind(ErrorKind kind, int expectedStatus)
    {
        var exception = new DomainException(new DomainError("SOME_ERROR", kind), "Message.");

        var (status, _) = ErrorMapper.Map(exception);

        Assert.Equal(expectedStatus, status);
    }

    [Fact]
    public void Map_DomainException_ReturnsItsCodeAndMessageWithoutFieldErrors()
    {
        var exception = new DomainException(new DomainError("SOME_CONFLICT", ErrorKind.Conflict), "It clashes.");

        var (_, body) = ErrorMapper.Map(exception);

        Assert.Equal("SOME_CONFLICT", body.Code);
        Assert.Equal("It clashes.", body.Message);
        Assert.Null(body.Errors);
    }

    [Fact]
    public void Map_UnexpectedException_Returns500WithoutInternalMessage()
    {
        var exception = new InvalidOperationException("Connection string leaked here");

        var (status, body) = ErrorMapper.Map(exception);

        Assert.Equal(500, status);
        Assert.Equal("INTERNAL_ERROR", body.Code);
        Assert.DoesNotContain("leaked", body.Message);
    }

    [Fact]
    public void FromModelState_JsonPathKeys_ReturnsInvalidInputWithCamelCaseFields()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("$.email", "'}' is an invalid start of a value.");
        modelState.AddModelError("", "A non-empty request body is required.");

        var body = ErrorMapper.FromModelState(modelState);

        Assert.Equal("INVALID_INPUT", body.Code);
        Assert.Equal(["'}' is an invalid start of a value."], body.Errors!["email"]);
        Assert.Equal(["A non-empty request body is required."], body.Errors["body"]);
    }
}
