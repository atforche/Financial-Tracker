using Models.FundGoals;
using Models.Funds;

namespace Models.Transactions.Types;

/// <summary>
/// Model representing a destination of a fund transaction response.
/// </summary>
public sealed class FundTransactionDestinationModel
{
    /// <summary>
    /// Fund for the destination.
    /// </summary>
    public required FundBalanceEventModel Fund { get; init; }

    /// <summary>
    /// Fund Goal balance event for the destination, when applicable.
    /// </summary>
    public FundGoalBalanceEventModel? FundGoal { get; init; }
}
