using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Respawn.Graph;
using TaskTracker.Infrastructure.Persistence;
using TaskTracker.Infrastructure.Persistence.Lookups;

namespace TaskTracker.IntegrationTests.Fixtures;

/// <summary>
/// Deletes every row except the migrations history and seeded lookup tables (ADR 0005).
/// The tables to keep come from the EF model, so a new lookup table only needs <see cref="ILookupTable"/>.
/// </summary>
public sealed class DatabaseReset
{
    private readonly string _connectionString;
    private readonly Respawner _respawner;

    private DatabaseReset(string connectionString, Respawner respawner)
    {
        _connectionString = connectionString;
        _respawner = respawner;
    }

    public static async Task<DatabaseReset> CreateAsync(string connectionString, AppDbContext db)
    {
        var defaultSchema = db.Model.GetDefaultSchema() ?? "dbo";
        var lookupTables = db.Model.GetEntityTypes()
            .Where(e => typeof(ILookupTable).IsAssignableFrom(e.ClrType))
            .Select(e => new Table(e.GetSchema() ?? defaultSchema, e.GetTableName()!));

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = [new Table("__EFMigrationsHistory"), .. lookupTables]
        });

        return new DatabaseReset(connectionString, respawner);
    }

    public async Task ResetAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }
}
