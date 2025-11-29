namespace Models.Errors;

/// <summary>
/// Model representing an error returned by the API
/// </summary>
public class ErrorModel
{
    /// <summary>
    /// Message for this Error
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Details for this Error
    /// </summary>
    public required IReadOnlyCollection<ErrorDetailModel> Details { get; init; }
}