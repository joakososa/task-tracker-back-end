using NJsonSchema;
using NJsonSchema.Validation;
using NSwag;

namespace TaskTracker.IntegrationTests.Contract;

/// <summary>
/// Provider-side contract testing: checks real responses against docs/openapi.yaml, so a renamed DTO property
/// or an undocumented status code fails a test even though the code compiles.
/// </summary>
public static class OpenApiContract
{
    private static readonly Lazy<Task<OpenApiDocument>> Document = new(() =>
        OpenApiYamlDocument.FromFileAsync(Path.Combine(AppContext.BaseDirectory, "openapi.yaml")));

    public static async Task AssertResponseMatchesAsync(HttpResponseMessage response)
    {
        var errors = await ValidateAsync(response);
        if (errors.Count > 0)
            Assert.Fail(string.Join(Environment.NewLine, errors));
    }

    public static async Task<IReadOnlyList<string>> ValidateAsync(HttpResponseMessage response)
    {
        var request = response.RequestMessage
            ?? throw new InvalidOperationException("The response has no request to match against the contract.");
        var method = request.Method.Method.ToLowerInvariant();
        var path = request.RequestUri!.AbsolutePath;
        var status = ((int)response.StatusCode).ToString();
        var endpoint = $"{request.Method.Method} {path}";

        var operation = FindOperation(await Document.Value, method, path);
        if (operation is null)
            return [$"{endpoint} is not in the contract."];

        if (!operation.ActualResponses.TryGetValue(status, out var documented))
            return [$"{endpoint} returned {status}, which the contract does not document."];

        // ActualResponse follows $ref to components/responses (e.g. ValidationError, Conflict).
        if (!documented.ActualResponse.Content.TryGetValue("application/json", out var mediaType))
            return [];

        var body = await response.Content.ReadAsStringAsync();
        return mediaType.Schema.ActualSchema.Validate(body, SchemaType.OpenApi3)
            .SelectMany(Flatten)
            .Select(error => $"{endpoint} {status}: {error.Kind} {error.Path}")
            .ToList();
    }

    // allOf/oneOf failures (e.g. UserResponse = AuditFields + own fields) wrap the real errors; report those.
    private static IEnumerable<ValidationError> Flatten(ValidationError error) =>
        error is ChildSchemaValidationError child
            ? child.Errors.Values.SelectMany(errors => errors).SelectMany(Flatten)
            : [error];

    // A literal path ("/users/me") wins over a template that also matches it ("/users/{id}").
    private static OpenApiOperation? FindOperation(OpenApiDocument document, string method, string path)
    {
        var pathItem = document.Paths.TryGetValue(path, out var exact)
            ? exact
            : document.Paths.FirstOrDefault(p => MatchesTemplate(p.Key, path)).Value;

        return pathItem is not null && pathItem.TryGetValue(method, out var operation) ? operation : null;
    }

    private static bool MatchesTemplate(string template, string path)
    {
        var templateSegments = template.Trim('/').Split('/');
        var pathSegments = path.Trim('/').Split('/');

        return templateSegments.Length == pathSegments.Length
            && templateSegments.Zip(pathSegments).All(s =>
                s.First.StartsWith('{') || string.Equals(s.First, s.Second, StringComparison.OrdinalIgnoreCase));
    }
}
