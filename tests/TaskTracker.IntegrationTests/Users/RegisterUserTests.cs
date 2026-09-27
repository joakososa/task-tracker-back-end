using System.Net;
using System.Net.Http.Json;
using System.Text;
using TaskTracker.Api.Errors;
using TaskTracker.Application.Users;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Users;

public class RegisterUserTests(IntegrationFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task PostUsers_WithValidInput_Returns201WithUser()
    {
        var response = await Client.PostAsJsonAsync("/users", new
        {
            name = "Ana",
            email = "ana@example.com",
            password = "password123",
            avatar = "https://example.com/ana.png"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);
        Assert.True(user.Id > 0);
        Assert.Equal("Ana", user.Name);
        Assert.Equal("ana@example.com", user.Email);
        Assert.Equal("https://example.com/ana.png", user.Avatar);
    }

    [Fact]
    public async Task PostUsers_WithExistingEmail_Returns409WithEmailAlreadyExistsCode()
    {
        var body = new { name = "Ana", email = "ana@example.com", password = "password123" };
        await Client.PostAsJsonAsync("/users", body);

        var response = await Client.PostAsJsonAsync("/users", body);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("USER_EMAIL_ALREADY_EXISTS", error?.Code);
    }

    [Fact]
    public async Task PostUsers_WithMissingNameAndShortPassword_Returns400WithBothFieldErrors()
    {
        var response = await Client.PostAsJsonAsync("/users", new { email = "ana@example.com", password = "short" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("INVALID_INPUT", error?.Code);
        Assert.Equal(["name", "password"], error?.Errors?.Keys.Order());
    }

    [Fact]
    public async Task PostUsers_WithMalformedJson_Returns400WithInvalidInputCode()
    {
        var content = new StringContent("{ \"name\": ", Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/users", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("INVALID_INPUT", error?.Code);
    }
}
