using System.Net;
using System.Net.Http.Json;
using TaskTracker.Api.Errors;
using TaskTracker.Application.Projects;
using TaskTracker.IntegrationTests.Contract;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Projects;

public class ListProjectsTests(IntegrationFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task GetProjects_UserWithOwnAndSharedProjects_Returns200WithBothInCreationOrder()
    {
        var bea = await AuthenticateAsync("bea@example.com");
        var own = await CreateProjectAsync("Propio de Bea");
        await AuthenticateAsync("ana@example.com");
        var shared = await CreateProjectAsync("Compartido por Ana");
        await CreateProjectAsync("Solo de Ana");
        await AddMemberInDatabaseAsync(shared.Id, bea.Id);
        await AuthenticateAsync("bea@example.com");

        var response = await Client.GetAsync("/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var projects = await response.Content.ReadFromJsonAsync<List<ProjectDto>>();
        Assert.Equal([own.Id, shared.Id], projects?.Select(p => p.Id));
    }

    [Fact]
    public async Task GetProjects_UserWithoutProjects_Returns200WithEmptyList()
    {
        await AuthenticateAsync("ana@example.com");
        await CreateProjectAsync();
        await AuthenticateAsync("carla@example.com");

        var response = await Client.GetAsync("/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var projects = await response.Content.ReadFromJsonAsync<List<ProjectDto>>();
        Assert.Empty(projects!);
    }

    [Fact]
    public async Task GetProjects_WithoutToken_Returns401WithUnauthorizedCode()
    {
        var response = await Client.GetAsync("/projects");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("UNAUTHORIZED", error?.Code);
    }
}
