using TaskTracker.Application.Common.Exceptions;

namespace TaskTracker.Application.Projects;

/// Errors shared by the project use cases, so every endpoint answers with the same code and message.
public static class ProjectErrors
{
    public static NotFoundException NotFound() => new("PROJECT_NOT_FOUND", "Project not found.");

    public static ForbiddenException Forbidden() =>
        new("PROJECT_FORBIDDEN", "You do not have permission for this action on this project.");

    /// 400, not 404: the project was found; what is wrong is the userId in the body.
    public static InvalidRequestException UserToAddNotFound() => new("USER_NOT_FOUND", "User does not exist.");
}
