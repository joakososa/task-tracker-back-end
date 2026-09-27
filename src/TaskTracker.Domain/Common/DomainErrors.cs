namespace TaskTracker.Domain.Common;

public static class DomainErrors
{
    public static readonly DomainError FieldRequired = new("FIELD_REQUIRED", ErrorKind.Validation);
    public static readonly DomainError FieldTooLong = new("FIELD_TOO_LONG", ErrorKind.Validation);

    public static class User
    {
        public static readonly DomainError EmailAlreadyExists = new("USER_EMAIL_ALREADY_EXISTS", ErrorKind.Conflict);
        public static readonly DomainError PasswordTooShort = new("PASSWORD_TOO_SHORT", ErrorKind.Validation);
        public static readonly DomainError PasswordTooLong = new("PASSWORD_TOO_LONG", ErrorKind.Validation);
    }

    public static class Project
    {
        public static readonly DomainError InvalidMember = new("PROJECT_INVALID_MEMBER", ErrorKind.Validation);
    }

    public static class TaskItem
    {
        public static readonly DomainError InvalidStateTransition = new("TASK_INVALID_STATE_TRANSITION", ErrorKind.Conflict);
    }
}