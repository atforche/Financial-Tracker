namespace Models.FundGoals;

/// <summary>
/// Model describing ending-balance progress for a Fund Goal.
/// </summary>
public sealed class FundGoalEndingBalanceProgressModel
{
    /// <summary>
    /// Gets the current balance.
    /// </summary>
    public required decimal CurrentBalance { get; init; }

    /// <summary>
    /// Gets the configured minimum ending balance.
    /// </summary>
    public decimal? MinimumBalance { get; init; }

    /// <summary>
    /// Gets the configured maximum ending balance.
    /// </summary>
    public decimal? MaximumBalance { get; init; }

    /// <summary>
    /// Gets the nonnegative amount below the minimum.
    /// </summary>
    public required decimal AmountBelowMinimum { get; init; }

    /// <summary>
    /// Gets the nonnegative amount above the maximum.
    /// </summary>
    public required decimal AmountAboveMaximum { get; init; }

    /// <summary>
    /// Gets the ending-balance status.
    /// </summary>
    public required FundGoalEndingBalanceStatusModel Status { get; init; }
}
