using System.Net;
using System.Net.Http.Json;
using TaskTracker.Api.Errors;
using TaskTracker.Application.Projects;
using TaskTracker.IntegrationTests.Contract;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Projects;

public class UpdateProjectTests(IntegrationFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task PutProject_AsOwner_Returns200WithNewName()
    {
        await AuthenticateAsync();
        var created = await CreateProjectAsync("Tablero");

        var response = await Client.PutAsJsonAsync($"/projects/{created.Id}", new { name = "Tablero nuevo" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.Equal(created.Id, project?.Id);
        Assert.Equal("Tablero nuevo", project?.Name);
    }

    [Fact]
    public async Task PutProject_ThenGet_ReturnsStoredNewName()
    {
        await AuthenticateAsync();
        var created = await CreateProjectAsync("Tablero");
        await Client.PutAsJsonAsync($"/projects/{created.Id}", new { name = "Tablero nuevo" });

        var project = await Client.GetFromJsonAsync<ProjectDto>($"/projects/{created.Id}");

        Assert.Equal("Tablero nuevo", project?.Name);
    }

    [Fact]
    public async Task PutProject_AsMember_Returns403WithProjectForbiddenCode()
    {
        await AuthenticateAsync("ana@example.com");
        var created = await CreateProjectAsync();
        var bea = await AuthenticateAsync("bea@example.com");
        await AddMemberInDatabaseAsync(created.Id, bea.Id);

        var response = await Client.PutAsJsonAsync($"/projects/{created.Id}", new { name = "Tablero nuevo" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("PROJECT_FORBIDDEN", error?.Code);
    }

    [Fact]
    public async Task PutProject_Missing_Returns404WithProjectNotFoundCode()
    {
        await AuthenticateAsync();

        var response = await Client.PutAsJsonAsync("/projects/999999", new { name = "Tablero nuevo" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("PROJECT_NOT_FOUND", error?.Code);
    }

    [Fact]
    public async Task PutProject_WithBlankName_Returns400WithNameError()
    {
        await AuthenticateAsync();
        var created = await CreateProjectAsync();

        var response = await Client.PutAsJsonAsync($"/projects/{created.Id}", new { name = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("INVALID_INPUT", error?.Code);
        Assert.Equal(["name"], error?.Errors?.Keys);
    }

    [Fact]
    public async Task PutProject_WithoutToken_Returns401WithUnauthorizedCode()
    {
        var response = await Client.PutAsJsonAsync("/projects/1", new { name = "Tablero nuevo" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("UNAUTHORIZED", error?.Code);
    }
}
