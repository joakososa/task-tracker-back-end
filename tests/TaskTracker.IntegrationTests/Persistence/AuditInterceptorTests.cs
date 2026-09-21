using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using TaskTracker.Domain.Entities;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Persistence;

[Collection(SqlServerCollection.Name)]
public class AuditInterceptorTests(SqlServerFixture fixture)
{
    private static readonly DateTimeOffset T0 = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SaveChanges_OnInsert_SetsCreatedAtUpdatedAtAndCreatedBy()
    {
        var clock = new FakeTimeProvider(T0);
        await using var db = fixture.CreateDbContext(currentUserId: 42, clock);
        var user = new User("U", $"{Guid.NewGuid():N}@example.com", "hash");

        db.Users.Add(user);
        await db.SaveChangesAsync();

        Assert.Equal(T0, user.CreatedAt);
        Assert.Equal(T0, user.UpdatedAt);
        Assert.Equal(42, user.CreatedBy);
    }

    [Fact]
    public async Task SaveChanges_OnInsertWithoutCurrentUser_LeavesCreatedByNull()
    {
        await using var db = fixture.CreateDbContext(currentUserId: null);
        var user = new User("U", $"{Guid.NewGuid():N}@example.com", "hash");

        db.Users.Add(user);
        await db.SaveChangesAsync();

        Assert.Null(user.CreatedBy);
    }

    [Fact]
    public async Task SaveChanges_OnUpdate_ChangesUpdatedAtButNotCreatedAtOrCreatedBy()
    {
        var clock = new FakeTimeProvider(T0);
        await using var db = fixture.CreateDbContext(currentUserId: 42, clock);
        var user = new User("U", $"{Guid.NewGuid():N}@example.com", "hash");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        clock.Advance(TimeSpan.FromHours(1));
        user.UpdateProfile("Renamed", null);
        await db.SaveChangesAsync();

        Assert.Equal(T0, user.CreatedAt);
        Assert.Equal(T0.AddHours(1), user.UpdatedAt);
        Assert.Equal(42, user.CreatedBy);
    }

    [Fact]
    public async Task SaveChanges_OnUpdateByAnotherUser_DoesNotChangeCreatedBy()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        int userId;
        await using (var db = fixture.CreateDbContext(currentUserId: 42))
        {
            var user = new User("U", email, "hash");
            db.Users.Add(user);
            await db.SaveChangesAsync();
            userId = user.Id;
        }

        await using (var db = fixture.CreateDbContext(currentUserId: 99))
        {
            var user = await db.Users.SingleAsync(u => u.Id == userId);
            user.UpdateProfile("Renamed", null);
            await db.SaveChangesAsync();
        }

        await using var verify = fixture.CreateDbContext(currentUserId: null);
        var reloaded = await verify.Users.SingleAsync(u => u.Id == userId);
        Assert.Equal(42, reloaded.CreatedBy);
    }
}