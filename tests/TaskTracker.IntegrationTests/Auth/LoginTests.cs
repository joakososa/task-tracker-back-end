using System.Net;
using System.Net.Http.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using TaskTracker.Api.Errors;
using TaskTracker.Application.Auth;
using TaskTracker.Application.Users;
using TaskTracker.IntegrationTests.Contract;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Auth;

public class LoginTests(IntegrationFixture fixture) : ApiTestBase(fixture)
{
    private async Task<UserDto> RegisterAnaAsync()
    {
        var response = await Client.PostAsJsonAsync("/users",
            new { name = "Ana", email = "ana@example.com", password = "password123" });
        return (await response.Content.ReadFromJsonAsync<UserDto>())!;
    }

    [Fact]
    public async Task PostAuthLogin_WithValidCredentials_Returns200WithTokenForTheUser()
    {
        var user = await RegisterAnaAsync();

        var response = await Client.PostAsJsonAsync("/auth/login",
            new { email = "ana@example.com", password = "password123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var token = await response.Content.ReadFromJsonAsync<AccessTokenDto>();
        Assert.Equal("Bearer", token?.TokenType);
        Assert.Equal(3600, token?.ExpiresIn);
        Assert.Equal(user.Id.ToString(), new JsonWebToken(token!.AccessToken).Subject);
    }

    [Fact]
    public async Task PostAuthLogin_WithWrongPassword_Returns401WithInvalidCredentialsCode()
    {
        await RegisterAnaAsync();

        var response = await Client.PostAsJsonAsync("/auth/login",
            new { email = "ana@example.com", password = "wrongpassword" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("INVALID_CREDENTIALS", error?.Code);
    }

    [Fact]
    public async Task PostAuthLogin_WithUnknownEmail_Returns401WithInvalidCredentialsCode()
    {
        var response = await Client.PostAsJsonAsync("/auth/login",
            new { email = "nobody@example.com", password = "password123" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("INVALID_CREDENTIALS", error?.Code);
    }

    [Fact]
    public async Task PostAuthLogin_WithMissingFields_Returns400WithBothFieldErrors()
    {
        var response = await Client.PostAsJsonAsync("/auth/login", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("INVALID_INPUT", error?.Code);
        Assert.Equal(["email", "password"], error?.Errors?.Keys.Order());
    }
}
