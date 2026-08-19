namespace Models;

/// <summary>
/// Model representing pagination parameters for API requests
/// </summary>
public class PaginationModel
{
    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}
