using System.Diagnostics.CodeAnalysis;
using TaskTracker.Domain.Common;

namespace TaskTracker.Domain.Entities;

public class Project : AuditableEntity
{
    public const int NameMaxLength = 200;

    private readonly List<ProjectMember> _members = [];

    public int Id { get; private set; }
    public string Name { get; private set; }
    public int OwnerId { get; private set; }

    public User Owner { get; private set; } = null!;
    public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();

#pragma warning disable CS8618 // EF Core materializes entities through this constructor and sets every property.
    private Project() { }
#pragma warning restore CS8618

    public Project(string name, int ownerId)
    {
        SetName(name);
        OwnerId = ownerId;
    }

    public void Rename(string name) => SetName(name);

    public bool IsOwner(int userId) => OwnerId == userId;
    public bool IsMember(int userId) => _members.Any(m => m.UserId == userId);
    public bool HasAccess(int userId) => IsOwner(userId) || IsMember(userId);

    public void AddMember(int userId)
    {
        if (IsOwner(userId))
            throw new DomainException(DomainErrors.Project.InvalidMember, "The project owner cannot be added as a member.");
        if (IsMember(userId))
            throw new DomainException(DomainErrors.Project.InvalidMember, "User is already a member of this project.");

        _members.Add(new ProjectMember(Id, userId));
    }

    [MemberNotNull(nameof(Name))]
    private void SetName(string name) =>
        Name = Guard.RequiredText(name, NameMaxLength, nameof(Name));
}