using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Errors;
using TaskTracker.Application.Projects;
using TaskTracker.Domain.Enums;
using TaskTracker.IntegrationTests.Contract;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Projects;

public class CreateProjectTests(IntegrationFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task PostProjects_WithName_Returns201WithProjectAndLocation()
    {
        var ana = await AuthenticateAsync();

        var response = await Client.PostAsJsonAsync("/projects", new { name = "Tablero" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
        Assert.NotNull(project);
        Assert.Equal("Tablero", project.Name);
        Assert.Equal(ana.Id, project.Owner.Id);
        Assert.Empty(project.Members);
        Assert.Equal(ana.Id, project.CreatedBy);
        Assert.Equal($"/projects/{project.Id}", response.Headers.Location?.OriginalString);
    }

    // Until GET /projects/{id} exists, the database is the only way to see what was stored.
    [Fact]
    public async Task PostProjects_WithName_StoresCurrentUserAsOwner()
    {
        var ana = await AuthenticateAsync();

        var response = await Client.PostAsJsonAsync("/projects", new { name = "Tablero" });

        var created = await response.Content.ReadFromJsonAsync<ProjectDto>();
        await using var db = Fixture.CreateDbContext();
        var stored = await db.Projects.Include(p => p.Members).SingleAsync(p => p.Id == created!.Id);
        Assert.Equal("Tablero", stored.Name);
        Assert.Equal(ProjectRole.Owner, stored.RoleOf(ana.Id));
        Assert.Single(stored.Members);
    }

    [Fact]
    public async Task PostProjects_WithBlankName_Returns400WithNameError()
    {
        await AuthenticateAsync();

        var response = await Client.PostAsJsonAsync("/projects", new { name = "   " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("INVALID_INPUT", error?.Code);
        Assert.Equal(["name"], error?.Errors?.Keys);
    }

    [Fact]
    public async Task PostProjects_WithoutToken_Returns401WithUnauthorizedCode()
    {
        var response = await Client.PostAsJsonAsync("/projects", new { name = "Tablero" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("UNAUTHORIZED", error?.Code);
    }
}
