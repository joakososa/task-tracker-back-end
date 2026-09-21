using TaskTracker.Domain.Common;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Entities;

public class TaskItem : AuditableEntity
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 4000;

    public int Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TaskPriority Priority { get; private set; }
    public TaskState State { get; private set; }
    public int ProjectId { get; private set; }
    public int? AssigneeId { get; private set; }

    public Project Project { get; private set; } = null!;
    public User? Assignee { get; private set; }

    private TaskItem() { }

    public TaskItem(int projectId, string title, string description, TaskPriority priority = TaskPriority.Medium)
    {
        ProjectId = projectId;
        SetDetails(title, description, priority);
        State = TaskState.Todo;
    }

    public void Update(string title, string description, TaskPriority priority) =>
        SetDetails(title, description, priority);

    private static readonly IReadOnlyDictionary<TaskState, TaskState[]> AllowedTransitions =
        new Dictionary<TaskState, TaskState[]>
        {
            [TaskState.Todo] = [TaskState.InProgress, TaskState.Done],
            [TaskState.InProgress] = [TaskState.Done],
            [TaskState.Done] = [],
        };

    public void ChangeState(TaskState newState)
    {
        if (!AllowedTransitions[State].Contains(newState))
            throw new DomainException(
                DomainErrors.TaskItem.InvalidStateTransition,
                $"Cannot move a task from {State} to {newState}.");

        State = newState;
    }

    public void AssignTo(int userId)
    {
        AssigneeId = userId;
    }

    private void SetDetails(string title, string description, TaskPriority priority)
    {
        Title = Guard.RequiredText(title, TitleMaxLength, nameof(Title));
        Description = Guard.RequiredText(description, DescriptionMaxLength, nameof(Description));
        Priority = priority;
    }
}