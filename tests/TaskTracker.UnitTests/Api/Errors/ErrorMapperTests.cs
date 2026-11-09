using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TaskTracker.Api.Errors;
using TaskTracker.Application.Common.Exceptions;
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
    public void Map_AuthenticationFailedException_Returns401WithInvalidCredentialsCode()
    {
        var (status, body) = ErrorMapper.Map(new AuthenticationFailedException());

        Assert.Equal(401, status);
        Assert.Equal("INVALID_CREDENTIALS", body.Code);
        Assert.Equal("Invalid email or password.", body.Message);
        Assert.Null(body.Errors);
    }

    [Fact]
    public void Map_NotAuthenticatedException_Returns401WithUnauthorizedCode()
    {
        var (status, body) = ErrorMapper.Map(new NotAuthenticatedException());

        Assert.Equal(401, status);
        Assert.Equal("UNAUTHORIZED", body.Code);
        Assert.Equal("Missing or invalid access token.", body.Message);
        Assert.Null(body.Errors);
    }

    [Fact]
    public void Map_NotFoundException_Returns404WithItsCodeAndMessage()
    {
        var (status, body) = ErrorMapper.Map(new NotFoundException("SOME_NOT_FOUND", "It is not there."));

        Assert.Equal(404, status);
        Assert.Equal("SOME_NOT_FOUND", body.Code);
        Assert.Equal("It is not there.", body.Message);
        Assert.Null(body.Errors);
    }

    [Fact]
    public void Map_ForbiddenException_Returns403WithItsCodeAndMessage()
    {
        var (status, body) = ErrorMapper.Map(new ForbiddenException("SOME_FORBIDDEN", "Not for you."));

        Assert.Equal(403, status);
        Assert.Equal("SOME_FORBIDDEN", body.Code);
        Assert.Equal("Not for you.", body.Message);
        Assert.Null(body.Errors);
    }

    [Fact]
    public void Map_InvalidRequestException_Returns400WithItsCodeAndMessage()
    {
        var (status, body) = ErrorMapper.Map(new InvalidRequestException("SOME_BAD_REFERENCE", "Points nowhere."));

        Assert.Equal(400, status);
        Assert.Equal("SOME_BAD_REFERENCE", body.Code);
        Assert.Equal("Points nowhere.", body.Message);
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
