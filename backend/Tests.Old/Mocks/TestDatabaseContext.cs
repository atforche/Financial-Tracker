using Data;

namespace Tests.Old.Mocks;

/// <summary>
/// Database Context for use in testing
/// </summary>
public class TestDatabaseContext : DatabaseContext
{
    /// <inheritdoc/>
    protected override string DatabasePath { get; } = Path.Join(DatabaseFixture.DatabaseDirectory, $"{Guid.NewGuid()}.db");
}