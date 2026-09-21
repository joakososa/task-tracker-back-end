using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Infrastructure.Persistence.Lookups;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("TaskItems", t =>
            t.HasCheckConstraint(
                "CK_TaskItems_Priority",
                $"[Priority] IN ({string.Join(", ", Enum.GetNames<TaskPriority>().Select(n => $"'{n}'"))})"));

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .HasMaxLength(TaskItem.TitleMaxLength);

        builder.Property(t => t.Description)
            .HasMaxLength(TaskItem.DescriptionMaxLength);

        // Priority: enum persisted as string + CHECK constraint (option b)
        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        // State: enum persisted as int with FK to a lookup table (option c)
        builder.HasOne<TaskStateLookup>()
            .WithMany()
            .HasForeignKey(t => t.State)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => new { t.ProjectId, t.State });
        builder.HasIndex(t => t.AssigneeId);
    }
}