using System.Reflection;

namespace Rest.Controllers;

/// <summary>
/// Static class for managing environment variables required by the Rest.Controllers assembly
/// </summary>
public static class EnvironmentManager
{
    /// <summary>
    /// Environment variable that stores the environment type that is running
    /// </summary>
    public static string EnvironmentType { get; } = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? throw new InvalidOperationException();

    /// <summary>
    /// Environment variable that stores the port that the backend should run on
    /// </summary>
    public static string Port { get; } = Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS") ?? throw new InvalidOperationException();

    /// <summary>
    /// Environment variables that stores the origin that frontend requests will come from
    /// </summary>
    public static string FrontendOrigin { get; } = Environment.GetEnvironmentVariable("FRONTEND_ORIGIN") ?? throw new InvalidOperationException();

    /// <summary>
    /// Prints a summary of the current environment variables
    /// </summary>
    public static void PrintEnvironment()
    {
        Console.WriteLine("Rest.Controllers Environment Variables:");
        foreach (PropertyInfo property in typeof(EnvironmentManager).GetProperties())
        {
            Console.WriteLine($"{property.Name}: {property.GetValue(null)}");
        }
        Console.WriteLine("");
    }
}