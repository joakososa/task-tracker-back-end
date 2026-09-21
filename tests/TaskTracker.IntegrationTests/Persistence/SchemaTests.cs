using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Infrastructure.Persistence;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Persistence;

[Collection(SqlServerCollection.Name)]
public class SchemaTests(SqlServerFixture fixture)
{
    [Fact]
    public async Task Model_HasNoPendingMigrations()
    {
        await using var db = fixture.CreateDbContext();

        var pending = await db.Database.GetPendingMigrationsAsync();

        Assert.Empty(pending);
    }

    [Fact]
    public async Task Model_MatchesLastMigrationSnapshot()
    {
        await using var db = fixture.CreateDbContext();

        var hasChanges = db.Database.HasPendingModelChanges();

        Assert.False(hasChanges, "The EF model differs from the last migration snapshot. Add a migration.");
    }

    [Fact]
    public async Task TaskStates_AreSeededFromEnum()
    {
        await using var db = fixture.CreateDbContext();

        var rows = await db.Database
            .SqlQueryRaw<string>("SELECT [Name] AS [Value] FROM [TaskStates] ORDER BY [Id]")
            .ToListAsync();

        Assert.Equal(Enum.GetNames<TaskState>(), rows);
    }

    [Fact]
    public async Task TaskItems_RejectPriorityOutsideCheckConstraint()
    {
        await using var db = fixture.CreateDbContext();
        var (userId, projectId) = await SeedUserAndProjectAsync(db);

        var ex = await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO [TaskItems] ([Title], [Description], [Priority], [State], [ProjectId], [CreatedAt], [UpdatedAt])
            VALUES (N'T', N'D', N'Banana', 0, {0}, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET())
            """, projectId));

        Assert.Contains("CK_TaskItems_Priority", ex.Message);
    }

    [Fact]
    public async Task TaskItems_RejectStateWithoutLookupRow()
    {
        await using var db = fixture.CreateDbContext();
        var (userId, projectId) = await SeedUserAndProjectAsync(db);

        var ex = await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO [TaskItems] ([Title], [Description], [Priority], [State], [ProjectId], [CreatedAt], [UpdatedAt])
            VALUES (N'T', N'D', N'Low', 99, {0}, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET())
            """, projectId));

        Assert.Contains("FK_TaskItems_TaskStates_State", ex.Message);
    }

    [Fact]
    public async Task Users_RejectDuplicateEmail()
    {
        await using var db = fixture.CreateDbContext();
        var email = $"{Guid.NewGuid():N}@example.com";
        await InsertUserAsync(db, email);

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => InsertUserAsync(db, email));

        Assert.Contains("IX_Users_Email", ex.InnerException?.Message);
    }

    private static async Task<(int userId, int projectId)> SeedUserAndProjectAsync(AppDbContext db)
    {
        var userId = await InsertUserAsync(db, $"{Guid.NewGuid():N}@example.com");

        var project = new Project("P", userId);
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        return (userId, project.Id);
    }

    private static async Task<int> InsertUserAsync(AppDbContext db, string email)
    {
        var user = new User("U", email, "hash");
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }
}
