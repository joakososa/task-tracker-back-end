namespace TaskTracker.IntegrationTests.Fixtures;

/// <summary>
/// Base for HTTP tests: every test starts from an empty database (lookup data kept) and talks to the in-memory Api.
/// </summary>
[Collection(IntegrationCollection.Name)]
public abstract class ApiTestBase(IntegrationFixture fixture) : IAsyncLifetime
{
    protected HttpClient Client { get; } = fixture.Api.CreateClient();

    public Task InitializeAsync() => fixture.ResetDatabaseAsync();

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }
}
