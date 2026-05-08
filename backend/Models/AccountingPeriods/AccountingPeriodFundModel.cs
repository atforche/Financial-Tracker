using Models.Funds;

namespace Models.AccountingPeriods;

/// <summary>
/// Model representing a Fund in the context of a specific Accounting Period
/// </summary>
public class AccountingPeriodFundModel : FundModel
{
    /// <summary>
    /// Opening balance for the Fund
    /// </summary>
    public required decimal OpeningBalance { get; init; }

    /// <summary>
    /// Amount assigned to the Fund for the Accounting Period
    /// </summary>
    public required decimal AmountAssigned { get; init; }

    /// <summary>
    /// Amount spent from the Fund during the Accounting Period
    /// </summary>
    public required decimal AmountSpent { get; init; }

    /// <summary>
    /// Closing balance for the Fund
    /// </summary>
    public required decimal ClosingBalance { get; init; }
}