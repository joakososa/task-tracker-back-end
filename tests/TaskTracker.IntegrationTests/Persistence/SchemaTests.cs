using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.Infrastructure.Persistence;
using TaskTracker.IntegrationTests.Fakes;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Persistence;

[Collection(IntegrationCollection.Name)]
public class SchemaTests(IntegrationFixture fixture)
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

    [Fact]
    public async Task ProjectMembers_RejectSecondOwner()
    {
        await using var db = fixture.CreateDbContext();
        var (_, projectId) = await SeedUserAndProjectAsync(db);
        var otherUserId = await InsertUserAsync(db, $"{Guid.NewGuid():N}@example.com");

        var ex = await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO [ProjectMembers] ([ProjectId], [UserId], [Role], [JoinedAt])
            VALUES ({0}, {1}, N'Owner', SYSDATETIMEOFFSET())
            """, projectId, otherUserId));

        Assert.Contains("IX_ProjectMembers_OneOwnerPerProject", ex.Message);
    }

    [Fact]
    public async Task ProjectMembers_RejectRoleOutsideCheckConstraint()
    {
        await using var db = fixture.CreateDbContext();
        var (_, projectId) = await SeedUserAndProjectAsync(db);
        var otherUserId = await InsertUserAsync(db, $"{Guid.NewGuid():N}@example.com");

        var ex = await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO [ProjectMembers] ([ProjectId], [UserId], [Role], [JoinedAt])
            VALUES ({0}, {1}, N'Admin', SYSDATETIMEOFFSET())
            """, projectId, otherUserId));

        Assert.Contains("CK_ProjectMembers_Role", ex.Message);
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
        var user = new User("U", email, "password", new FakePasswordHasher());
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }
}
