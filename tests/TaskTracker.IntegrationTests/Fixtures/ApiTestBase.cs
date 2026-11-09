using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.Auth;
using TaskTracker.Application.Projects;
using TaskTracker.Application.Users;

namespace TaskTracker.IntegrationTests.Fixtures;

/// <summary>
/// Base for HTTP tests: every test starts from an empty database (lookup data kept) and talks to the in-memory Api.
/// </summary>
[Collection(IntegrationCollection.Name)]
public abstract class ApiTestBase(IntegrationFixture fixture) : IAsyncLifetime
{
    protected HttpClient Client { get; } = fixture.Api.CreateClient();

    /// For test data the Api cannot create yet, straight in the database.
    protected IntegrationFixture Fixture { get; } = fixture;

    public Task InitializeAsync() => Fixture.ResetDatabaseAsync();

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    /// Registers a user and logs in through the Api; every later request of the test carries the token.
    protected async Task<UserDto> AuthenticateAsync(string email = "ana@example.com")
    {
        const string password = "password123";
        var registered = await Client.PostAsJsonAsync("/users", new { name = "Ana", email, password });
        var user = (await registered.Content.ReadFromJsonAsync<UserDto>())!;

        var login = await Client.PostAsJsonAsync("/auth/login", new { email, password });
        var token = (await login.Content.ReadFromJsonAsync<AccessTokenDto>())!;
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        return user;
    }

    /// Creates a project through the Api as the currently authenticated user.
    protected async Task<ProjectDto> CreateProjectAsync(string name = "Tablero")
    {
        var response = await Client.PostAsJsonAsync("/projects", new { name });
        return (await response.Content.ReadFromJsonAsync<ProjectDto>())!;
    }

    /// Straight in the database until POST /projects/{id}/members exists.
    protected async Task AddMemberInDatabaseAsync(int projectId, int userId)
    {
        await using var db = Fixture.CreateDbContext();
        var project = await db.Projects.Include(p => p.Members).SingleAsync(p => p.Id == projectId);
        project.AddMember(userId);
        await db.SaveChangesAsync();
    }
}
