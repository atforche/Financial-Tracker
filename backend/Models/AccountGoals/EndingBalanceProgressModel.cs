namespace Models.AccountGoals;

/// <summary>
/// Model describing ending-balance progress for an Account Goal.
/// </summary>
public sealed class EndingBalanceProgressModel
{
    /// <summary>
    /// Gets the current Account balance.
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
    /// Gets the amount below the configured minimum.
    /// </summary>
    public required decimal AmountBelowMinimum { get; init; }

    /// <summary>
    /// Gets the amount above the configured maximum.
    /// </summary>
    public required decimal AmountAboveMaximum { get; init; }

    /// <summary>
    /// Gets the ending-balance status.
    /// </summary>
    public required EndingBalanceStatusModel Status { get; init; }
}
