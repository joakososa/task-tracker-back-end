using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using TaskTracker.IntegrationTests.Fakes;
using TaskTracker.IntegrationTests.Fixtures;

namespace TaskTracker.IntegrationTests.Persistence;

[Collection(IntegrationCollection.Name)]
public class ProjectMappingTests(IntegrationFixture fixture)
{
    [Fact]
    public async Task SaveChanges_NewProject_PersistsOwnerAsMemberRow()
    {
        int ownerId, projectId;
        await using (var db = fixture.CreateDbContext())
        {
            ownerId = await InsertUserAsync(db);
            var project = new Project("P", ownerId);
            db.Projects.Add(project);
            await db.SaveChangesAsync();
            projectId = project.Id;
        }

        await using var verify = fixture.CreateDbContext();
        var reloaded = await verify.Projects.Include(p => p.Members).SingleAsync(p => p.Id == projectId);

        var member = Assert.Single(reloaded.Members);
        Assert.Equal(ownerId, member.UserId);
        Assert.Equal(projectId, member.ProjectId);
        Assert.Equal(ProjectRole.Owner, member.Role);
    }

    [Fact]
    public async Task SaveChanges_AddMemberToLoadedProject_PersistsMemberRole()
    {
        int memberId, projectId;
        await using (var db = fixture.CreateDbContext())
        {
            var project = new Project("P", await InsertUserAsync(db));
            db.Projects.Add(project);
            await db.SaveChangesAsync();
            projectId = project.Id;
            memberId = await InsertUserAsync(db);
        }

        await using (var db = fixture.CreateDbContext())
        {
            var project = await db.Projects.Include(p => p.Members).SingleAsync(p => p.Id == projectId);
            project.AddMember(memberId);
            await db.SaveChangesAsync();
        }

        await using var verify = fixture.CreateDbContext();
        var reloaded = await verify.Projects.Include(p => p.Members).SingleAsync(p => p.Id == projectId);
        Assert.Equal(2, reloaded.Members.Count);
        Assert.Equal(ProjectRole.Member, reloaded.RoleOf(memberId));
    }

    private static async Task<int> InsertUserAsync(DbContext db)
    {
        var user = new User("U", $"{Guid.NewGuid():N}@example.com", "password", new FakePasswordHasher());
        db.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }
}
