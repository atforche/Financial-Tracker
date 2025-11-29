namespace Models.Errors;

/// <summary>
/// Model representing a piece of detailed information about an error
/// </summary>
public class ErrorDetailModel
{
    /// <summary>
    /// Error Code for this Error Detail
    /// </summary>
    public required ErrorCode ErrorCode { get; init; }

    /// <summary>
    /// Description for this Error Detail
    /// </summary>
    public required string Description { get; init; }
}