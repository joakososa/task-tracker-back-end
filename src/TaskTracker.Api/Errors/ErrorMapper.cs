using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TaskTracker.Domain.Common;

namespace TaskTracker.Api.Errors;

/// Turns anything that can go wrong in a request into a status code and an ErrorResponse (see ADR 0007).
public static class ErrorMapper
{
    public const string InvalidInputCode = "INVALID_INPUT";
    public const string InternalErrorCode = "INTERNAL_ERROR";

    public static (int Status, ErrorResponse Body) Map(Exception exception) => exception switch
    {
        ValidationException validation => (StatusCodes.Status400BadRequest, new ErrorResponse(
            InvalidInputCode, "One or more fields are invalid.", GroupByField(validation.Errors))),

        DomainException domain => (StatusFor(domain.Kind), new ErrorResponse(domain.Code, domain.Message)),

        // Never expose the internal message or stack trace; it is logged instead.
        _ => (StatusCodes.Status500InternalServerError, new ErrorResponse(
            InternalErrorCode, "An unexpected error occurred."))
    };

    /// For requests rejected by model binding (malformed JSON, wrong types, missing body) before any handler runs.
    public static ErrorResponse FromModelState(ModelStateDictionary modelState) => new(
        InvalidInputCode,
        "The request body is invalid.",
        modelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .ToDictionary(
                entry => ToFieldName(entry.Key),
                entry => entry.Value!.Errors.Select(e => e.ErrorMessage).ToArray()));

    private static int StatusFor(ErrorKind kind) => kind switch
    {
        ErrorKind.Validation => StatusCodes.Status400BadRequest,
        ErrorKind.Conflict => StatusCodes.Status409Conflict,
        ErrorKind.NotFound => StatusCodes.Status404NotFound,
        ErrorKind.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError
    };

    private static Dictionary<string, string[]> GroupByField(IEnumerable<ValidationFailure> failures) =>
        failures
            .GroupBy(f => ToFieldName(f.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

    // "Email" (FluentValidation) and "$.email" (System.Text.Json) both become "email", as in the JSON body.
    private static string ToFieldName(string key)
    {
        var name = key.StartsWith("$.") ? key[2..] : key;
        return name.Length == 0 ? "body" : JsonNamingPolicy.CamelCase.ConvertName(name);
    }
}
