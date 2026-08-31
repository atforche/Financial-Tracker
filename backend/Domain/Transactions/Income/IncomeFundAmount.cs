using Domain.Funds;

namespace Domain.Transactions.Income;

/// <summary>
/// Value object representing a Fund amount assigned by an income transaction.
/// </summary>
public sealed class IncomeFundAmount : FundAmount
{
    /// <summary>
    /// Whether this assignment is extra funding rather than an expected monthly
    /// contribution toward the Fund Goal.
    /// </summary>
    public bool IsExtraContribution { get; init; }
}
