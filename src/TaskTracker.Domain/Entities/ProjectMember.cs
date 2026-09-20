namespace TaskTracker.Domain.Entities;

public class ProjectMember
{
    public int ProjectId { get; private set; }
    public int UserId { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }

    public User User { get; private set; } = null!;

    private ProjectMember() { }

    internal ProjectMember(int projectId, int userId)
    {
        ProjectId = projectId;
        UserId = userId;
        JoinedAt = DateTimeOffset.UtcNow;
    }
}