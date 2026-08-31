using Domain.AccountingPeriods;

namespace Domain.Funds;

/// <summary>
/// Record representing a request to create a <see cref="Fund"/>
/// </summary>
public record CreateFundRequest
{
    /// <summary>
    /// Name for the Fund
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description for the Fund
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Opening Accounting Period for the Fund
    /// </summary>
    public required AccountingPeriod OpeningAccountingPeriod { get; init; }

    /// <summary>
    /// Regular contribution for the Fund Goal.
    /// </summary>
    public required decimal? RegularContribution { get; init; }

    /// <summary>
    /// Minimum ending balance for the Fund Goal.
    /// </summary>
    public required decimal? MinimumEndingBalance { get; init; }

    /// <summary>
    /// Maximum ending balance for the Fund Goal.
    /// </summary>
    public required decimal? MaximumEndingBalance { get; init; }
}
