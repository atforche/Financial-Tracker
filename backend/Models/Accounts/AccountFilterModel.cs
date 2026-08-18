namespace Models.Accounts;

/// <summary>
/// Model representing the filters that can be applied when retrieving Accounts
/// </summary>
public class AccountFilterModel
{
    /// <summary>
    /// Search to apply to the results
    /// </summary>
    public string? NameSearch { get; init; }

    /// <summary>
    /// Account Name filters to apply to the results.
    /// </summary>
    public List<string>? Names { get; init; }

    /// <summary>
    /// Account Type filters to apply to the results.
    /// </summary>
    public List<AccountTypeModel>? Types { get; init; }
}
