using System.Net;
using System.Net.Http.Json;
using TaskTracker.Application.Users;
using TaskTracker.IntegrationTests.Contract;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Users;

public class GetCurrentUserTests(IntegrationFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task GetUsersMe_WithValidToken_Returns200WithCurrentUser()
    {
        var registered = await AuthenticateAsync("ana@example.com");

        var response = await Client.GetAsync("/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.Equal(registered.Id, user?.Id);
        Assert.Equal("Ana", user?.Name);
        Assert.Equal("ana@example.com", user?.Email);
    }
}
