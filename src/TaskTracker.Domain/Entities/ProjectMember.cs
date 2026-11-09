using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Entities;

public class ProjectMember
{
    public int ProjectId { get; private set; }
    public int UserId { get; private set; }
    public ProjectRole Role { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }

    public User User { get; private set; } = null!;

    private ProjectMember() { }

    // No projectId: members are only created through Project's collection, and EF sets the
    // foreign key on save. That also works for a new project whose Id is not generated yet.
    internal ProjectMember(int userId, ProjectRole role)
    {
        UserId = userId;
        Role = role;
        JoinedAt = DateTimeOffset.UtcNow;
    }
}
