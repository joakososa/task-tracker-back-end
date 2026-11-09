namespace TaskTracker.Application.Projects.AddMember;

public sealed record AddProjectMemberCommand(int ProjectId, int UserId);
