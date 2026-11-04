using System.Net;
using System.Net.Http.Json;
using TaskTracker.Api.Errors;
using TaskTracker.Application.Users;
using TaskTracker.IntegrationTests.Contract;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Users;

public class UpdateCurrentUserTests(IntegrationFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task PutUsersMe_WithNameAndAvatar_Returns200WithUpdatedUser()
    {
        var registered = await AuthenticateAsync();

        var response = await Client.PutAsJsonAsync("/users/me",
            new { name = "Ana Gómez", avatar = "https://example.com/ana.png" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.Equal(registered.Id, user?.Id);
        Assert.Equal("Ana Gómez", user?.Name);
        Assert.Equal("https://example.com/ana.png", user?.Avatar);
    }

    [Fact]
    public async Task PutUsersMe_ThenGetUsersMe_ReturnsPersistedChanges()
    {
        await AuthenticateAsync();
        await Client.PutAsJsonAsync("/users/me", new { name = "Ana Gómez", avatar = "https://example.com/ana.png" });

        var user = await Client.GetFromJsonAsync<UserDto>("/users/me");

        Assert.Equal("Ana Gómez", user?.Name);
        Assert.Equal("https://example.com/ana.png", user?.Avatar);
    }

    [Fact]
    public async Task PutUsersMe_WithoutAvatar_RemovesAvatar()
    {
        await AuthenticateAsync();
        await Client.PutAsJsonAsync("/users/me", new { name = "Ana", avatar = "https://example.com/ana.png" });

        var response = await Client.PutAsJsonAsync("/users/me", new { name = "Ana" });

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.Null(user?.Avatar);
    }

    [Fact]
    public async Task PutUsersMe_WithoutName_Returns400WithNameError()
    {
        await AuthenticateAsync();

        var response = await Client.PutAsJsonAsync("/users/me", new { avatar = "https://example.com/ana.png" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("INVALID_INPUT", error?.Code);
        Assert.Equal(["name"], error?.Errors?.Keys);
    }

    [Fact]
    public async Task PutUsersMe_WithoutToken_Returns401()
    {
        var response = await Client.PutAsJsonAsync("/users/me", new { name = "Ana" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
    }
}
