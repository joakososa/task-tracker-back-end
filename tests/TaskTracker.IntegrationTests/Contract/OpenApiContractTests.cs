using System.Net;
using System.Text;

namespace TaskTracker.IntegrationTests.Contract;

/// A checker that never reports anything would still let every test pass, so it gets tests of its own.
public class OpenApiContractTests
{
    [Fact]
    public async Task ValidateAsync_BodyMissingRequiredProperty_ReturnsError()
    {
        var response = Response(HttpStatusCode.Created, """
            { "name": "Ana", "email": "ana@example.com",
              "createdAt": "2026-01-15T10:00:00+00:00", "updatedAt": "2026-01-15T10:00:00+00:00" }
            """);

        var errors = await OpenApiContract.ValidateAsync(response);

        Assert.Equal(["POST /users 201: PropertyRequired #/id"], errors);
    }

    [Fact]
    public async Task ValidateAsync_UndocumentedStatus_ReturnsError()
    {
        var response = Response(HttpStatusCode.InternalServerError, """{ "code": "INTERNAL_ERROR", "message": "x" }""");

        var errors = await OpenApiContract.ValidateAsync(response);

        Assert.Equal(["POST /users returned 500, which the contract does not document."], errors);
    }

    private static HttpResponseMessage Response(HttpStatusCode status, string body) => new(status)
    {
        RequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://localhost/users"),
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };
}
