using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Infrastructure.Persistence.Configurations;

public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.ToTable("ProjectMembers", t =>
            t.HasCheckConstraint(
                "CK_ProjectMembers_Role",
                $"[Role] IN ({string.Join(", ", Enum.GetNames<ProjectRole>().Select(n => $"'{n}'"))})"));

        builder.HasKey(m => new { m.ProjectId, m.UserId });

        // Role: enum persisted as string + CHECK constraint, like TaskPriority (ADR 0009).
        builder.Property(m => m.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Exactly one owner per project, enforced by the database.
        builder.HasIndex(m => m.ProjectId)
            .IsUnique()
            .HasFilter($"[Role] = '{nameof(ProjectRole.Owner)}'")
            .HasDatabaseName("IX_ProjectMembers_OneOwnerPerProject");

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
