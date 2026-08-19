namespace Data;

/// <summary>
/// Abstract base class for managing environment variables
/// </summary>
public abstract class EnvironmentVariableManager
{
    /// <summary>
    /// Gets the value of an environment variable.
    /// </summary>
    /// <param name="name">Name of the environment variable to retrieve</param>
    /// <returns>The value of the environment variable</returns>
    protected static string GetEnvironmentVariable(string name) =>
        Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException($"Environment variable '{name}' is not set.");
}
