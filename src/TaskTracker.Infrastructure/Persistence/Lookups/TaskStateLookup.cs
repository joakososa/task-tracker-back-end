using TaskTracker.Domain.Enums;

namespace TaskTracker.Infrastructure.Persistence.Lookups;

public class TaskStateLookup : ILookupTable
{
    public TaskState Id { get; set; }
    public string Name { get; set; } = null!;
}