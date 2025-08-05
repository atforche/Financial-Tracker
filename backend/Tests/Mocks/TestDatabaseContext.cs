using Data;

namespace Tests.Mocks;

/// <summary>
/// Database Context for use in testing
/// </summary>
public class TestDatabaseContext : DatabaseContext
{
    /// <inheritdoc/>
    protected override string DatabasePath => Path.Join(DatabaseFixture.DatabaseDirectory, $"{Guid.NewGuid()}.db");
}