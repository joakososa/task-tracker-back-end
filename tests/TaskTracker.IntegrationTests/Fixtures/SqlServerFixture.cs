using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Infrastructure.Persistence;
using TaskTracker.Infrastructure.Persistence.Interceptors;
using Testcontainers.MsSql;

namespace TaskTracker.IntegrationTests.Fixtures;

/// <summary>
/// Starts one SQL Server container for the whole test run, applies the EF migrations once,
/// and hands out fresh <see cref="AppDbContext"/> instances pointing at it.
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

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
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SqlServer";
}
