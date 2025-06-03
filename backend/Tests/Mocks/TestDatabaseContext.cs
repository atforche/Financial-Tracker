using Data;

namespace Tests.Mocks;

/// <summary>
/// Database Context for use in testing
/// </summary>
public class TestDatabaseContext : DatabaseContext
{
    /// <summary>
    /// Gets the directory where the test databases will be located
    /// </summary>
    public static string DatabaseDirectory => $"{AppContext.BaseDirectory}/Database";

    /// <inheritdoc/>
    protected override string GetDatabasePath() => Path.Join(DatabaseDirectory, $"{Guid.NewGuid()}.db");
}