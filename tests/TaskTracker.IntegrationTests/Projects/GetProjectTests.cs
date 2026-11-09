using System.Net;
using System.Net.Http.Json;
using TaskTracker.Api.Errors;
using TaskTracker.Application.Projects;
using TaskTracker.IntegrationTests.Contract;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Projects;

public class GetProjectTests(IntegrationFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task GetProject_AsOwner_Returns200WithOwnerAndMembers()
    {
        var bea = await AuthenticateAsync("bea@example.com");
        var ana = await AuthenticateAsync("ana@example.com");
        var created = await CreateProjectAsync("Tablero");
        await AddMemberAsync(created.Id, bea.Id);

        var response = await Client.GetAsync($"/projects/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.Equal("Tablero", project?.Name);
        Assert.Equal(ana.Id, project?.Owner.Id);
        Assert.Equal([bea.Id], project?.Members.Select(m => m.Id));
    }

    [Fact]
    public async Task GetProject_AsMember_Returns200()
    {
        var bea = await AuthenticateAsync("bea@example.com");
        await AuthenticateAsync("ana@example.com");
        var created = await CreateProjectAsync();
        await AddMemberAsync(created.Id, bea.Id);
        await LoginAsync("bea@example.com");

        var response = await Client.GetAsync($"/projects/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
    }

    [Fact]
    public async Task GetProject_AsStranger_Returns403WithProjectForbiddenCode()
    {
        await AuthenticateAsync("ana@example.com");
        var created = await CreateProjectAsync();
        await AuthenticateAsync("carla@example.com");

        var response = await Client.GetAsync($"/projects/{created.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("PROJECT_FORBIDDEN", error?.Code);
    }

    [Fact]
    public async Task GetProject_Missing_Returns404WithProjectNotFoundCode()
    {
        await AuthenticateAsync();

        var response = await Client.GetAsync("/projects/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("PROJECT_NOT_FOUND", error?.Code);
    }

    [Fact]
    public async Task GetProject_WithoutToken_Returns401WithUnauthorizedCode()
    {
        var response = await Client.GetAsync("/projects/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("UNAUTHORIZED", error?.Code);
    }
}
