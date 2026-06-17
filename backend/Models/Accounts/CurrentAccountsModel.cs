namespace Models.Accounts;

/// <summary>
/// Model representing the current Accounts page response.
/// </summary>
public class CurrentAccountsModel
{
    /// <summary>
    /// Current aggregate summary for all Accounts.
    /// </summary>
    public required AccountSummaryModel Summary { get; init; }

    /// <summary>
    /// Current snapshot for each Account.
    /// </summary>
    public required IReadOnlyCollection<CurrentAccountModel> Accounts { get; init; }
}