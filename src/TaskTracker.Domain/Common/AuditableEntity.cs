namespace TaskTracker.Domain.Common;

/// Audit fields are set only by the persistence layer (AuditInterceptor, through EF), never by domain code.
public abstract class AuditableEntity
{
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int? CreatedBy { get; private set; }
}
