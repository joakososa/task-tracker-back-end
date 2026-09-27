using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Infrastructure.Persistence;
using TaskTracker.Infrastructure.Persistence.Interceptors;
using Testcontainers.MsSql;

namespace TaskTracker.IntegrationTests.Fixtures;

/// <summary>
/// Starts one SQL Server container for the whole test run, applies the EF migrations once,
/// and hands out fresh <see cref="AppDbContext"/> instances pointing at it.
/// It also owns the in-memory Api and the database reset, one of each per run
/// (xUnit v2 collection fixtures cannot depend on each other).
/// </summary>
public sealed class IntegrationFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    private DatabaseReset _reset = null!;

    public string ConnectionString => _container.GetConnectionString();

    public ApiFactory Api { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();

        _reset = await DatabaseReset.CreateAsync(ConnectionString, db);
        Api = new ApiFactory(ConnectionString);
    }

    public async Task DisposeAsync()
    {
        await Api.DisposeAsync();
        await _container.DisposeAsync();
    }

    public Task ResetDatabaseAsync() => _reset.ResetAsync();

    public AppDbContext CreateDbContext(int? currentUserId = null, TimeProvider? clock = null)
    {
        var interceptor = new AuditInterceptor(new FixedCurrentUser(currentUserId), clock ?? TimeProvider.System);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .AddInterceptors(interceptor)
            .Options;

        return new AppDbContext(options);
    }
    private sealed class FixedCurrentUser(int? userId) : ICurrentUser
    {
        public int? UserId => userId;
    }
}

[CollectionDefinition(Name)]
public sealed class IntegrationCollection : ICollectionFixture<IntegrationFixture>
{
    public const string Name = "Integration";
}
