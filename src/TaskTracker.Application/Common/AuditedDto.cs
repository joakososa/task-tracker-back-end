namespace TaskTracker.Application.Common;

public abstract record AuditedDto(DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, int? CreatedBy)
    : TimestampedDto(CreatedAt, UpdatedAt);