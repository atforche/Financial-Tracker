using Models.FundGoals;
using Models.Funds;

namespace Models.Transactions.Types;

/// <summary>
/// Model representing the source of a fund transaction response.
/// </summary>
public sealed class FundTransactionSourceModel
{
    /// <summary>
    /// Fund for the source.
    /// </summary>
    public required FundBalanceEventModel Fund { get; init; }

    /// <summary>
    /// Fund Goal balance event for the source, when applicable.
    /// </summary>
    public FundGoalBalanceEventModel? FundGoal { get; init; }
}