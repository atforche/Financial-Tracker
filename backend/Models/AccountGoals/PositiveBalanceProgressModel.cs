namespace Models.AccountGoals;

/// <summary>
/// Model describing positive-balance health for an Account Goal.
/// </summary>
public sealed class PositiveBalanceProgressModel
{
    /// <summary>
    /// Gets the current Account balance.
    /// </summary>
    public required decimal CurrentBalance { get; init; }

    /// <summary>
    /// Gets whether the current balance is strictly greater than zero.
    /// </summary>
    public required bool IsSatisfied { get; init; }
}
