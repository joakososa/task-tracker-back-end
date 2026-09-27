namespace TaskTracker.Api.Errors;

/// The `Error` schema from docs/openapi.yaml. `Errors` is only present for field validation failures.
public sealed record ErrorResponse(string Code, string Message, IReadOnlyDictionary<string, string[]>? Errors = null);
