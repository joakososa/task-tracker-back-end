namespace TaskTracker.Domain.Common;

public static class DomainErrors
{
    
    public const string FieldRequired = "FIELD_REQUIRED";
    public static class Project
    {
        public const string InvalidMember = "PROJECT_INVALID_MEMBER";     
    }

    public static class TaskItem
    {
        public const string InvalidStateTransition = "TASK_INVALID_STATE_TRANSITION";
    }
}