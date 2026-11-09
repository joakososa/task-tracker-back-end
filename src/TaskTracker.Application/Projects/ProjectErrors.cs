using TaskTracker.Application.Common.Exceptions;

namespace TaskTracker.Application.Projects;

/// Errors shared by the project use cases, so every endpoint answers with the same code and message.
public static class ProjectErrors
{
    public static NotFoundException NotFound() => new("PROJECT_NOT_FOUND", "Project not found.");

    public static ForbiddenException Forbidden() =>
        new("PROJECT_FORBIDDEN", "You do not have permission for this action on this project.");
}
