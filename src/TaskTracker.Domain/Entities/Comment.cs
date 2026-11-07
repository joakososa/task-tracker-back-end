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

#pragma warning disable CS8618 // EF Core materializes entities through this constructor and sets every property.
    private Comment() { }
#pragma warning restore CS8618

    public Comment(int taskItemId, int authorId, string content)
    {
        TaskItemId = taskItemId;
        AuthorId = authorId;
        Content = Guard.RequiredText(content, ContentMaxLength, nameof(Content));
    }
}