using System.Net.Http.Headers;
using System.Net.Http.Json;
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

    private const string Password = "password123";

    /// Registers a new user and logs in through the Api; every later request of the test carries the token.
    /// Only for new emails: to switch back to a user the test already registered, use <see cref="LoginAsync"/>.
    protected async Task<UserDto> AuthenticateAsync(string email = "ana@example.com")
    {
        var registered = await Client.PostAsJsonAsync("/users", new { name = "Ana", email, password = Password });
        registered.EnsureSuccessStatusCode();
        var user = (await registered.Content.ReadFromJsonAsync<UserDto>())!;

        await LoginAsync(email);
        return user;
    }

    /// Logs in as a user the test already registered; every later request of the test carries the token.
    protected async Task LoginAsync(string email)
    {
        var login = await Client.PostAsJsonAsync("/auth/login", new { email, password = Password });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<AccessTokenDto>())!;
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
    }

    /// Creates a project through the Api as the currently authenticated user.
    protected async Task<ProjectDto> CreateProjectAsync(string name = "Tablero")
    {
        var response = await Client.PostAsJsonAsync("/projects", new { name });
        return (await response.Content.ReadFromJsonAsync<ProjectDto>())!;
    }

    /// Adds a member through the Api; the current user must be the project's owner.
    protected async Task AddMemberAsync(int projectId, int userId)
    {
        var response = await Client.PostAsJsonAsync($"/projects/{projectId}/members", new { userId });
        response.EnsureSuccessStatusCode();
    }
}
