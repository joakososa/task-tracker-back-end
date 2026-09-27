namespace TaskTracker.Application.Common;

public abstract record TimestampedDto(DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);