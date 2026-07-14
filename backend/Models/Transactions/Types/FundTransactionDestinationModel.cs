using Models.BalanceEvents;

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
    /// Goal for the destination, if applicable.
    /// </summary>
    public GoalBalanceEventModel? Goal { get; init; }
}