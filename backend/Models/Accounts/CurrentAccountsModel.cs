namespace Models.Accounts;

/// <summary>
/// Model representing the current Accounts page response.
/// </summary>
public class CurrentAccountsModel
{
    /// <summary>
    /// Available Account Names for the current snapshot filters.
    /// </summary>
    public required IReadOnlyCollection<string> AvailableAccountNames { get; init; }

    /// <summary>
    /// Current aggregate summary for the matching Accounts.
    /// </summary>
    public required AccountSummaryModel Summary { get; init; }

    /// <summary>
    /// Current snapshot for each Account.
    /// </summary>
    public required IReadOnlyCollection<CurrentAccountModel> Accounts { get; init; }
}