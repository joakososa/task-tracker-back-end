using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using TaskTracker.Api.Errors;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure.Security;
using TaskTracker.IntegrationTests.Contract;
using TaskTracker.IntegrationTests.Fakes;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Auth;

/// Requests the Api must reject before any handler runs, plus the endpoints that stay open without a token.
public class AuthorizationTests(IntegrationFixture fixture) : ApiTestBase(fixture)
{
    // Builds a token outside the Api, so a test can choose the signing key, the issue time and the user.
    private static string CreateToken(string signingKey, DateTimeOffset issuedAt, int userId)
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "TaskTracker",
            Audience = "TaskTracker",
            SigningKey = signingKey,
            LifetimeMinutes = 60
        });
        var user = new User("Ana", "ana@example.com", "password123", new FakePasswordHasher());
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, userId);

        return new JwtTokenService(options, new FakeTimeProvider(issuedAt)).CreateAccessToken(user).AccessToken;
    }

    private async Task AssertUnauthorizedAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("UNAUTHORIZED", error?.Code);
    }

    private Task<HttpResponseMessage> GetUsersMeWithTokenAsync(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return Client.GetAsync("/users/me");
    }

    [Fact]
    public async Task GetUsersMe_WithoutToken_Returns401WithUnauthorizedCode()
    {
        var response = await Client.GetAsync("/users/me");

        await AssertUnauthorizedAsync(response);
    }

    [Fact]
    public async Task GetUsersMe_WithMalformedToken_Returns401WithUnauthorizedCode()
    {
        var response = await GetUsersMeWithTokenAsync("not-a-jwt");

        await AssertUnauthorizedAsync(response);
    }

    [Fact]
    public async Task GetUsersMe_WithTokenSignedWithAnotherKey_Returns401WithUnauthorizedCode()
    {
        var user = await AuthenticateAsync();
        var token = CreateToken("another-signing-key-of-at-least-32-bytes", DateTimeOffset.UtcNow, user.Id);

        var response = await GetUsersMeWithTokenAsync(token);

        await AssertUnauthorizedAsync(response);
    }

    [Fact]
    public async Task GetUsersMe_WithExpiredToken_Returns401WithUnauthorizedCode()
    {
        var user = await AuthenticateAsync();
        // Issued two hours ago with a 60-minute lifetime: expired an hour ago, far beyond the clock skew.
        var token = CreateToken(ApiFactory.SigningKey, DateTimeOffset.UtcNow.AddHours(-2), user.Id);

        var response = await GetUsersMeWithTokenAsync(token);

        await AssertUnauthorizedAsync(response);
    }

    [Fact]
    public async Task GetUsersMe_WithValidTokenForMissingUser_Returns401WithUnauthorizedCode()
    {
        var token = CreateToken(ApiFactory.SigningKey, DateTimeOffset.UtcNow, userId: 999);

        var response = await GetUsersMeWithTokenAsync(token);

        await AssertUnauthorizedAsync(response);
    }

    [Fact]
    public async Task GetHealth_WithoutToken_Returns200()
    {
        var response = await Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
