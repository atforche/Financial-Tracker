namespace Models.Funds;

/// <summary>
/// Model representing the filters that can be applied when retrieving Funds.
/// </summary>
public class FundFilterModel
{
    /// <summary>
    /// Search to apply to the results.
    /// </summary>
    public string? NameSearch { get; init; }

    /// <summary>
    /// Fund name filters to apply to the results.
    /// </summary>
    public List<string>? Names { get; init; }
}
