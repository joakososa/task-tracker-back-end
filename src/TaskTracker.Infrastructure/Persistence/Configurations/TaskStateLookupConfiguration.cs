using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Domain.Enums;
using TaskTracker.Infrastructure.Persistence.Lookups;

namespace TaskTracker.Infrastructure.Persistence.Configurations;

public class TaskStateLookupConfiguration : IEntityTypeConfiguration<TaskStateLookup>
{
    public void Configure(EntityTypeBuilder<TaskStateLookup> builder)
    {
        builder.ToTable("TaskStates");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.Name)
            .HasMaxLength(20);

        builder.HasData(
            Enum.GetValues<TaskState>()
                .Select(s => new TaskStateLookup { Id = s, Name = s.ToString() }));
    }
}