using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TaskTracker.Infrastructure.Persistence;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Persistence;

/// Data migrations with existing rows. Each test uses its own database on the shared container,
/// so the migrated-down schema never touches the database the other tests use.
[Collection(IntegrationCollection.Name)]
public class MigrationTests(IntegrationFixture fixture)
{
    private const string InitialCreate = "20260921054459_InitialCreate";
    private const string ProjectMemberRoles = "20261109162013_ProjectMemberRoles";

    [Fact]
    public async Task ProjectMemberRoles_Up_MovesOwnerIntoProjectMembers()
    {
        await using var db = CreateIsolatedDbContext();
        var migrator = db.GetService<IMigrator>();
        try
        {
            await migrator.MigrateAsync(InitialCreate);
            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO [Users] ([Name], [Email], [PasswordHash], [CreatedAt], [UpdatedAt])
                VALUES (N'Owner', N'owner@example.com', N'h', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),
                       (N'Member', N'member@example.com', N'h', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
                INSERT INTO [Projects] ([Name], [OwnerId], [CreatedAt], [UpdatedAt])
                VALUES (N'P', 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
                INSERT INTO [ProjectMembers] ([ProjectId], [UserId], [JoinedAt])
                VALUES (1, 2, SYSDATETIMEOFFSET());
                """);

            await migrator.MigrateAsync(ProjectMemberRoles);

            var rows = await db.Database
                .SqlQueryRaw<string>(
                    "SELECT CONCAT([UserId], ':', [Role]) AS [Value] FROM [ProjectMembers] WHERE [ProjectId] = 1 ORDER BY [UserId]")
                .ToListAsync();
            Assert.Equal(["1:Owner", "2:Member"], rows);
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public async Task ProjectMemberRoles_Down_RestoresOwnerIdAndRemovesOwnerRow()
    {
        await using var db = CreateIsolatedDbContext();
        var migrator = db.GetService<IMigrator>();
        try
        {
            await migrator.MigrateAsync(ProjectMemberRoles);
            await db.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO [Users] ([Name], [Email], [PasswordHash], [CreatedAt], [UpdatedAt])
                VALUES (N'Owner', N'owner@example.com', N'h', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()),
                       (N'Member', N'member@example.com', N'h', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
                INSERT INTO [Projects] ([Name], [CreatedAt], [UpdatedAt])
                VALUES (N'P', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET());
                INSERT INTO [ProjectMembers] ([ProjectId], [UserId], [Role], [JoinedAt])
                VALUES (1, 1, N'Owner', SYSDATETIMEOFFSET()),
                       (1, 2, N'Member', SYSDATETIMEOFFSET());
                """);

            await migrator.MigrateAsync(InitialCreate);

            var ownerId = await db.Database
                .SqlQueryRaw<int>("SELECT [OwnerId] AS [Value] FROM [Projects] WHERE [Id] = 1")
                .SingleAsync();
            var memberIds = await db.Database
                .SqlQueryRaw<int>("SELECT [UserId] AS [Value] FROM [ProjectMembers] WHERE [ProjectId] = 1")
                .ToListAsync();
            Assert.Equal(1, ownerId);
            Assert.Equal([2], memberIds);
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
        }
    }

    private AppDbContext CreateIsolatedDbContext()
    {
        var connectionString = new SqlConnectionStringBuilder(fixture.ConnectionString)
        {
            InitialCatalog = $"Migration_{Guid.NewGuid():N}",
        }.ConnectionString;

        return new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connectionString).Options);
    }
}
