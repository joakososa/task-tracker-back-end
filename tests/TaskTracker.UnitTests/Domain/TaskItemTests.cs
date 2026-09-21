using TaskTracker.Domain.Common;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.UnitTests.Domain;

public class TaskItemTests
{
    private static TaskItem CreateTask(TaskState state = TaskState.Todo)
    {
        var task = new TaskItem(projectId: 1, title: "Title", description: "Description");
        if (state != TaskState.Todo)
            task.ChangeState(state);
        return task;
    }

    [Fact]
    public void Constructor_SetsInitialStateToTodo_Succeeds()
    {
        var task = new TaskItem(1, "Title", "Description");

        Assert.Equal(TaskState.Todo, task.State);
    }

    [Fact]
    public void Constructor_DefaultsPriorityToMedium_Succeeds()
    {
        var task = new TaskItem(1, "Title", "Description");

        Assert.Equal(TaskPriority.Medium, task.Priority);
    }

    [Fact]
    public void Constructor_WithTitleOverMaxLength_ThrowsFieldTooLong()
    {
        var title = new string('a', TaskItem.TitleMaxLength + 1);

        var ex = Assert.Throws<DomainException>(() => new TaskItem(1, title, "Description"));

        Assert.Equal(DomainErrors.FieldTooLong, ex.Code);
    }

    [Fact]
    public void Constructor_WithDescriptionOverMaxLength_ThrowsFieldTooLong()
    {
        var description = new string('a', TaskItem.DescriptionMaxLength + 1);

        var ex = Assert.Throws<DomainException>(() => new TaskItem(1, "Title", description));

        Assert.Equal(DomainErrors.FieldTooLong, ex.Code);
    }

    [Theory]
    [InlineData(TaskState.Todo, TaskState.InProgress)]
    [InlineData(TaskState.Todo, TaskState.Done)]
    [InlineData(TaskState.InProgress, TaskState.Done)]
    public void ChangeState_ForwardTransition_Succeeds(TaskState from, TaskState to)
    {
        var task = CreateTask(from);

        task.ChangeState(to);

        Assert.Equal(to, task.State);
    }

    [Theory]
    [InlineData(TaskState.Todo, TaskState.Todo)]
    [InlineData(TaskState.InProgress, TaskState.Todo)]
    [InlineData(TaskState.InProgress, TaskState.InProgress)]
    [InlineData(TaskState.Done, TaskState.Todo)]
    [InlineData(TaskState.Done, TaskState.InProgress)]
    [InlineData(TaskState.Done, TaskState.Done)]
    public void ChangeState_BackwardOrSameTransition_Throws(TaskState from, TaskState to)
    {
        var task = CreateTask(from);

        var ex = Assert.Throws<DomainException>(() => task.ChangeState(to));

        Assert.Equal(DomainErrors.TaskItem.InvalidStateTransition, ex.Code);
        Assert.Equal(from, task.State);
    }

    [Fact]
    public void Update_WithTitleOverMaxLength_ThrowsFieldTooLongAndKeepsOriginalValues()
    {
        var title = new string('a', TaskItem.TitleMaxLength + 1);
        var task = CreateTask();

        var ex = Assert.Throws<DomainException>(() => task.Update(title, "Description", TaskPriority.Medium));

        Assert.Equal(DomainErrors.FieldTooLong, ex.Code);
        Assert.Equal("Title", task.Title);
    }

    [Fact]
    public void Update_WithDescriptionOverMaxLength_ThrowsFieldTooLongAndKeepsOriginalValues()
    {
        var description = new string('a', TaskItem.DescriptionMaxLength + 1);
        var task = CreateTask();

        var ex = Assert.Throws<DomainException>(() => task.Update("Title", description, TaskPriority.Medium));

        Assert.Equal(DomainErrors.FieldTooLong, ex.Code);
        Assert.Equal("Description", task.Description);
    }

    [Fact]
    public void Update_SetsTitleDescriptionAndPriority()
    {
        var task = CreateTask();

        task.Update("New title", "New description", TaskPriority.High);

        Assert.Equal("New title", task.Title);
        Assert.Equal("New description", task.Description);
        Assert.Equal(TaskPriority.High, task.Priority);
    }

    [Fact]
    public void AssignTo_SetsAssigneeId()
    {
        var task = CreateTask();

        task.AssignTo(7);

        Assert.Equal(7, task.AssigneeId);
    }

    [Fact]
    public void AssignTo_WhenAlreadyAssigned_ReplacesAssignee()
    {
        var task = CreateTask();
        task.AssignTo(7);

        task.AssignTo(8);

        Assert.Equal(8, task.AssigneeId);
    }
}
