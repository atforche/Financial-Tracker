using System.Reflection;

namespace Data;

/// <summary>
/// Static class for managing environment variables required by the Data assembly
/// </summary>
public static class EnvironmentManager
{
    /// <summary>
    /// Environment variable that stores the path to the database file
    /// </summary>
    public static string DatabasePath { get; } = Environment.GetEnvironmentVariable("DATABASE_PATH") ?? throw new InvalidOperationException();

    /// <summary>
    /// Prints a summary of the current environment variables
    /// </summary>
    public static void PrintEnvironment()
    {
        Console.WriteLine("Data Environment Variables:");
        foreach (PropertyInfo property in typeof(EnvironmentManager).GetProperties())
        {
            Console.WriteLine($"{property.Name}: {property.GetValue(null)}");
        }
        Console.WriteLine("");
    }
}