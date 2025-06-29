[assembly: AssemblyFixture(typeof(Tests.DatabaseFixture))]

namespace Tests;

/// <summary>
/// XUnit assembly fixture class for managing creation and cleanup of databases
/// </summary>
public class DatabaseFixture : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// True if this test run should use a real database, false otherwise
    /// </summary>
    public static bool ShouldUseDatabase { get; } = Environment.GetEnvironmentVariable("USE_DATABASE") == "TRUE";

    /// <summary>
    /// Gets the directory where the test databases will be located
    /// </summary>
    public static string DatabaseDirectory { get; } = $"{AppContext.BaseDirectory}/Database";

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public DatabaseFixture()
    {
        if (ShouldUseDatabase && !Directory.Exists(DatabaseDirectory))
        {
            Directory.CreateDirectory(DatabaseDirectory);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing && ShouldUseDatabase)
        {
            Directory.Delete(DatabaseDirectory, true);
        }
        _disposed = true;
    }
}