using System.Net;
using System.Net.Http.Json;
using TaskTracker.Api.Errors;
using TaskTracker.Application.Projects;
using TaskTracker.IntegrationTests.Contract;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Projects;

public class AddProjectMemberTests(IntegrationFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task PostMembers_AsOwner_Returns204()
    {
        var bea = await AuthenticateAsync("bea@example.com");
        await AuthenticateAsync("ana@example.com");
        var created = await CreateProjectAsync();

        var response = await Client.PostAsJsonAsync($"/projects/{created.Id}/members", new { userId = bea.Id });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
    }

    [Fact]
    public async Task PostMembers_ThenGetProject_ListsNewMember()
    {
        var bea = await AuthenticateAsync("bea@example.com");
        await AuthenticateAsync("ana@example.com");
        var created = await CreateProjectAsync();
        await Client.PostAsJsonAsync($"/projects/{created.Id}/members", new { userId = bea.Id });

        var project = await Client.GetFromJsonAsync<ProjectDto>($"/projects/{created.Id}");

        Assert.Equal([bea.Id], project?.Members.Select(m => m.Id));
    }

    [Fact]
    public async Task PostMembers_UserAlreadyMember_Returns409WithMemberAlreadyExistsCode()
    {
        var bea = await AuthenticateAsync("bea@example.com");
        await AuthenticateAsync("ana@example.com");
        var created = await CreateProjectAsync();
        await AddMemberAsync(created.Id, bea.Id);

        var response = await Client.PostAsJsonAsync($"/projects/{created.Id}/members", new { userId = bea.Id });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("PROJECT_MEMBER_ALREADY_EXISTS", error?.Code);
    }

    [Fact]
    public async Task PostMembers_UserIsOwner_Returns409WithMemberAlreadyExistsCode()
    {
        var ana = await AuthenticateAsync("ana@example.com");
        var created = await CreateProjectAsync();

        var response = await Client.PostAsJsonAsync($"/projects/{created.Id}/members", new { userId = ana.Id });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("PROJECT_MEMBER_ALREADY_EXISTS", error?.Code);
    }

    [Fact]
    public async Task PostMembers_MissingUser_Returns400WithUserNotFoundCode()
    {
        await AuthenticateAsync();
        var created = await CreateProjectAsync();

        var response = await Client.PostAsJsonAsync($"/projects/{created.Id}/members", new { userId = 999999 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("USER_NOT_FOUND", error?.Code);
    }

    [Fact]
    public async Task PostMembers_WithoutUserId_Returns400WithUserIdError()
    {
        await AuthenticateAsync();
        var created = await CreateProjectAsync();

        var response = await Client.PostAsJsonAsync($"/projects/{created.Id}/members", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("INVALID_INPUT", error?.Code);
        Assert.Equal(["userId"], error?.Errors?.Keys);
    }

    [Fact]
    public async Task PostMembers_AsMember_Returns403WithProjectForbiddenCode()
    {
        var bea = await AuthenticateAsync("bea@example.com");
        var carla = await AuthenticateAsync("carla@example.com");
        await AuthenticateAsync("ana@example.com");
        var created = await CreateProjectAsync();
        await AddMemberAsync(created.Id, bea.Id);
        await LoginAsync("bea@example.com");

        var response = await Client.PostAsJsonAsync($"/projects/{created.Id}/members", new { userId = carla.Id });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("PROJECT_FORBIDDEN", error?.Code);
    }

    [Fact]
    public async Task PostMembers_MissingProject_Returns404WithProjectNotFoundCode()
    {
        var bea = await AuthenticateAsync("bea@example.com");
        await AuthenticateAsync("ana@example.com");

        var response = await Client.PostAsJsonAsync("/projects/999999/members", new { userId = bea.Id });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("PROJECT_NOT_FOUND", error?.Code);
    }

    [Fact]
    public async Task PostMembers_WithoutToken_Returns401WithUnauthorizedCode()
    {
        var response = await Client.PostAsJsonAsync("/projects/1/members", new { userId = 1 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("UNAUTHORIZED", error?.Code);
    }
}
