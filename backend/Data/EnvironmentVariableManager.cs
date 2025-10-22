using System.Reflection;
using Serilog;

namespace Data;

/// <summary>
/// Abstract base class for managing environment variables
/// </summary>
public abstract class EnvironmentVariableManager
{
    /// <summary>
    /// Prints a summary of the current environment variables
    /// </summary>
    public void PrintEnvironment()
    {
        Log.Information($"{GetType().Assembly.GetName()} Environment Variables:");
        foreach (PropertyInfo property in GetType().GetProperties().Where(property => property.Name != "Instance"))
        {
            Log.Information($"{property.Name}: {property.GetValue(this)}");
        }
        Log.Information("");
    }

    /// <summary>
    /// Returns true if the current process is running as a part of MSBuild, false otherwise
    /// </summary>
    public static bool IsRunningInMsBuild() => Environment.GetEnvironmentVariable("MSBuildSDKsPath") is not null;

    /// <summary>
    /// Gets the value of an environment variable.
    /// </summary>
    /// <param name="name">Name of the environment variable to retrieve</param>
    /// <returns>The value of the environment variable</returns>
    protected static string GetEnvironmentVariable(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        if (value is null && !IsRunningInMsBuild())
        {
            // Environment variables are required unless we're running as a part of MSBuild
            throw new InvalidOperationException($"Environment variable '{name}' is not set.");
        }
        return value ?? "";
    }
}