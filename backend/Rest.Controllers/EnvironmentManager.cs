namespace Rest.Controllers;

/// <summary>
/// Class for managing environment variables required by the Rest.Controllers assembly
/// </summary>
public class EnvironmentManager : Data.EnvironmentVariableManager
{
    private static readonly Lazy<EnvironmentManager> _instance = new(() => new EnvironmentManager());

    /// <summary>
    /// Environment variable that stores the environment type that is running
    /// </summary>
    public string EnvironmentType { get; }

    /// <summary>
    /// Environment variable that stores the port that the backend should run on
    /// </summary>
    public string Port { get; }

    /// <summary>
    /// Environment variables that stores the origin that frontend requests will come from
    /// </summary>
    public string FrontendOrigin { get; }

    /// <summary>
    /// Environment variable that stores the directory where logs should be written
    /// </summary>
    public string LogDirectory { get; }

    /// <summary>
    /// Gets the singleton instance of EnvironmentManager
    /// </summary>
    public static EnvironmentManager Instance => _instance.Value;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    private EnvironmentManager()
    {
        EnvironmentType = GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        Port = GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS");
        FrontendOrigin = GetEnvironmentVariable("FRONTEND_ORIGIN");
        LogDirectory = GetEnvironmentVariable("LOG_DIRECTORY");
    }
}