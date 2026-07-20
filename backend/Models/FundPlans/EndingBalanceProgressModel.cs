namespace Models.FundPlans;

/// <summary>
/// Model describing ending-balance progress.
/// </summary>
public sealed class EndingBalanceProgressModel
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
    public required EndingBalanceStatusModel Status { get; init; }

    /// <summary>
    /// Gets the projected ending balance when available.
    /// </summary>
    public decimal? ProjectedEndingBalance { get; init; }
}