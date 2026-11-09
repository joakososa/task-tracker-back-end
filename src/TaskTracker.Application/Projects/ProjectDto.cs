using TaskTracker.Application.Common;
using TaskTracker.Application.Users;

namespace TaskTracker.Application.Projects;

/// Owner and members are separate (contract ProjectResponse): `Members` holds only the Member role, so nobody
/// appears twice.
public sealed record ProjectDto(
    int Id,
    string Name,
    UserDto Owner,
    IReadOnlyList<UserDto> Members,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int? CreatedBy)
    : AuditedDto(CreatedAt, UpdatedAt, CreatedBy);
