using System.Diagnostics.CodeAnalysis;
using TaskTracker.Domain.Common;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Entities;

public class Project : AuditableEntity
{
    public const int NameMaxLength = 200;

    private readonly List<ProjectMember> _members = [];

    public int Id { get; private set; }
    public string Name { get; private set; }

    public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();

#pragma warning disable CS8618 // EF Core materializes entities through this constructor and sets every property.
    private Project() { }
#pragma warning restore CS8618

    public Project(string name, int ownerId)
    {
        SetName(name);
        _members.Add(new ProjectMember(ownerId, ProjectRole.Owner));
    }

    public void Rename(string name) => SetName(name);

    /// The user's role in this project, or null when the user is not in it.
    /// Requires <see cref="Members"/> to be loaded: on a project read without them, everyone is a stranger.
    public ProjectRole? RoleOf(int userId) =>
        _members.SingleOrDefault(m => m.UserId == userId)?.Role;

    public bool Can(int userId, ProjectPermission permission) =>
        RoleOf(userId)?.Grants(permission) ?? false;

    public void AddMember(int userId)
    {
        if (RoleOf(userId) is not null)
            throw new DomainException(DomainErrors.Project.MemberAlreadyExists, "User is already a member of this project.");

        _members.Add(new ProjectMember(userId, ProjectRole.Member));
    }

    [MemberNotNull(nameof(Name))]
    private void SetName(string name) =>
        Name = Guard.RequiredText(name, NameMaxLength, nameof(Name));
}
