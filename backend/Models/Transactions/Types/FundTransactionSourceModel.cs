using Models.BalanceEvents;

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
    /// Goal for the source, if applicable.
    /// </summary>
    public GoalBalanceEventModel? Goal { get; init; }
}