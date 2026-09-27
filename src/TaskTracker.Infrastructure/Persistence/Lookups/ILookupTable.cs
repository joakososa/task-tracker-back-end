namespace TaskTracker.Infrastructure.Persistence.Lookups;

/// Marks seeded reference tables. Tests keep their rows when resetting the database (ADR 0005).
public interface ILookupTable;
