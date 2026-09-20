using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Entities;

public class Comment : AuditableEntity
{
    public const int ContentMaxLength = 2000;

    public int Id { get; private set; }
    public string Content { get; private set; }
    public int TaskItemId { get; private set; }
    public int AuthorId { get; private set; }

    public User Author { get; private set; } = null!;

    private Comment() { }

    public Comment(int taskItemId, int authorId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException(DomainErrors.FieldRequired, "Comment content is required.");

        TaskItemId = taskItemId;
        AuthorId = authorId;
        Content = content.Trim();
    }
}