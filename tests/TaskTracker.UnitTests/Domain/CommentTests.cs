using TaskTracker.Domain.Common;
using TaskTracker.Domain.Entities;

namespace TaskTracker.UnitTests.Domain;

public class CommentTests
{
    [Fact]
    public void Constructor_SetsTaskAndAuthor()
    {
        var comment = new Comment(taskItemId: 10, authorId: 3, content: "Looks good");

        Assert.Equal(10, comment.TaskItemId);
        Assert.Equal(3, comment.AuthorId);
        Assert.Equal("Looks good", comment.Content);
    }

    [Fact]
    public void Constructor_TrimsContent()
    {
        var comment = new Comment(10, 3, "  Looks good  ");

        Assert.Equal("Looks good", comment.Content);
    }

    [Fact]
    public void Constructor_WithContentOverMaxLength_ThrowsFieldTooLong()
    {
        var content = new string('a', Comment.ContentMaxLength + 1);

        var ex = Assert.Throws<DomainException>(() => new Comment(10, 3, content));

        Assert.Equal(DomainErrors.FieldTooLong, ex.Code);
    }
}
