using System.Diagnostics.CodeAnalysis;

namespace TaskTracker.Domain.Enums;

/// What a use case asks for; roles grant permissions (ADR 0009). Added when a use case needs one.
[SuppressMessage("Naming", "CA1711",
    Justification = "Domain term from ADR 0009; the reserved suffix is for Code Access Security types, which .NET no longer has.")]
public enum ProjectPermission
{
    ViewProject = 0,
    EditProject = 1,
    DeleteProject = 2,
    ManageMembers = 3
}
