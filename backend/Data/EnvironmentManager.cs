namespace Data;

/// <summary>
/// Class for managing environment variables required by the Data assembly
/// </summary>
public class EnvironmentManager : EnvironmentVariableManager
{
    private static readonly Lazy<EnvironmentManager> _instance = new(() => new EnvironmentManager());

    /// <summary>
    /// Environment variable that stores the path to the database file
    /// </summary>
    public string DatabasePath { get; }

    /// <summary>
    /// Gets the singleton instance of EnvironmentManager
    /// </summary>
    public static EnvironmentManager Instance => _instance.Value;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    private EnvironmentManager() => DatabasePath = GetEnvironmentVariable("DATABASE_PATH");
}