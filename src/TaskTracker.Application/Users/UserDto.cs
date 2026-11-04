using TaskTracker.Domain.Entities;
using TaskTracker.Application.Common;

namespace TaskTracker.Application.Users;

public sealed record UserDto(
 int Id,
 string Name,
 string Email,
 string? Avatar,
 DateTimeOffset CreatedAt,
 DateTimeOffset UpdatedAt)
    : TimestampedDto(CreatedAt, UpdatedAt)
{
    public static UserDto FromEntity(User user) => new(
        user.Id,
        user.Name,
        user.Email,
        user.AvatarUrl,
        user.CreatedAt,
        user.UpdatedAt);
}