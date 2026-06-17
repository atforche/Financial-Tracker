namespace Models.Accounts;

/// <summary>
/// Model representing the query parameters for the current Accounts endpoint.
/// </summary>
public class CurrentAccountsQueryParameterModel
{
    /// <summary>
    /// Optional Account Type filters to apply to the current snapshot.
    /// </summary>
    public List<AccountTypeModel>? AccountType { get; init; }

    /// <summary>
    /// Optional Account Name filters to apply to the current snapshot.
    /// </summary>
    public List<string>? AccountName { get; init; }
}