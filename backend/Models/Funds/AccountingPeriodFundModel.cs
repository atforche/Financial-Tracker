namespace Models.Funds;

/// <summary>
/// Model representing a Fund in the context of a specific Accounting Period
/// </summary>
public class AccountingPeriodFundModel
{
    /// <summary>
    /// ID for the Fund
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Name for the Fund
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type of the Fund
    /// </summary>
    public required FundTypeModel Type { get; init; }

    /// <summary>
    /// Description for the Fund
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Opening balance for the Fund
    /// </summary>
    public required FundBalanceModel OpeningBalance { get; init; }

    /// <summary>
    /// Closing balance for the Fund
    /// </summary>
    public required FundBalanceModel ClosingBalance { get; init; }
}