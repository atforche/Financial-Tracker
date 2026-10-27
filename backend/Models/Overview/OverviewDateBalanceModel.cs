namespace Models.Overview;

/// <summary>
/// Model representing a total balance at the end of a date.
/// </summary>
public class OverviewDateBalanceModel
{
    /// <summary>
    /// Date of the balance.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Total posted balance at the end of the date.
    /// </summary>
    public required decimal TotalBalance { get; init; }
}
