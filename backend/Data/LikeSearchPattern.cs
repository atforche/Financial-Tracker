namespace Data;

/// <summary>
/// Builds a SQL LIKE pattern that matches search text literally within a value.
/// </summary>
internal static class LikeSearchPattern
{
    public const string EscapeCharacter = "\\";

    public static string Contains(string value) =>
        $"%{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal)}%";
}
