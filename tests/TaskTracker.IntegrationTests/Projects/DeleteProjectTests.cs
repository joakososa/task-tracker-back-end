using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Errors;
using TaskTracker.Domain.Entities;
using TaskTracker.IntegrationTests.Contract;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Projects;

public class DeleteProjectTests(IntegrationFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task DeleteProject_AsOwner_Returns204()
    {
        await AuthenticateAsync();
        var created = await CreateProjectAsync();

        var response = await Client.DeleteAsync($"/projects/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
    }

    [Fact]
    public async Task DeleteProject_ThenGet_Returns404()
    {
        await AuthenticateAsync();
        var created = await CreateProjectAsync();
        await Client.DeleteAsync($"/projects/{created.Id}");

        var response = await Client.GetAsync($"/projects/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Tasks and comments have no endpoints yet, so they go straight to the database. The point is that
    // deleting a project is not blocked by any Restrict foreign key and leaves nothing behind.
    [Fact]
    public async Task DeleteProject_WithMembersTasksAndComments_DeletesThemAll()
    {
        var bea = await AuthenticateAsync("bea@example.com");
        var ana = await AuthenticateAsync("ana@example.com");
        var created = await CreateProjectAsync();
        await AddMemberInDatabaseAsync(created.Id, bea.Id);
        await using (var db = Fixture.CreateDbContext())
        {
            var task = new TaskItem(created.Id, "Tarea", "Descripción");
            db.TaskItems.Add(task);
            await db.SaveChangesAsync();
            db.Comments.Add(new Comment(task.Id, ana.Id, "Comentario"));
            await db.SaveChangesAsync();
        }

        var response = await Client.DeleteAsync($"/projects/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await using var check = Fixture.CreateDbContext();
        Assert.Equal(0, await check.ProjectMembers.CountAsync());
        Assert.Equal(0, await check.TaskItems.CountAsync());
        Assert.Equal(0, await check.Comments.CountAsync());
        Assert.Equal(2, await check.Users.CountAsync());
    }

    [Fact]
    public async Task DeleteProject_AsMember_Returns403WithProjectForbiddenCode()
    {
        await AuthenticateAsync("ana@example.com");
        var created = await CreateProjectAsync();
        var bea = await AuthenticateAsync("bea@example.com");
        await AddMemberInDatabaseAsync(created.Id, bea.Id);

        var response = await Client.DeleteAsync($"/projects/{created.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("PROJECT_FORBIDDEN", error?.Code);
    }

    [Fact]
    public async Task DeleteProject_Missing_Returns404WithProjectNotFoundCode()
    {
        await AuthenticateAsync();

        var response = await Client.DeleteAsync("/projects/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("PROJECT_NOT_FOUND", error?.Code);
    }

    [Fact]
    public async Task DeleteProject_WithoutToken_Returns401WithUnauthorizedCode()
    {
        var response = await Client.DeleteAsync("/projects/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await OpenApiContract.AssertResponseMatchesAsync(response);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("UNAUTHORIZED", error?.Code);
    }
}
