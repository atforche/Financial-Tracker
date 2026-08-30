namespace Models.FundGoals;

/// <summary>
/// Model describing ending-balance progress for a Fund Goal.
/// </summary>
public sealed class FundGoalEndingBalanceProgressModel
{
    /// <summary>
    /// Gets the target ending balance.
    /// </summary>
    public required decimal TargetBalance { get; init; }

    /// <summary>
    /// Gets the current balance.
    /// </summary>
    public required decimal CurrentBalance { get; init; }

    /// <summary>
    /// Gets current balance minus target balance.
    /// </summary>
    public required decimal Variance { get; init; }

    /// <summary>
    /// Gets the ending-balance status.
    /// </summary>
    public required FundGoalEndingBalanceStatusModel Status { get; init; }

    /// <summary>
    /// Gets the projected ending balance when available.
    /// </summary>
    public decimal? ProjectedEndingBalance { get; init; }
}
