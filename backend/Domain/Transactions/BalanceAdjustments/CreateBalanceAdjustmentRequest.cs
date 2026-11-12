using Domain.Accounts;

namespace Domain.Transactions.BalanceAdjustments;

/// <summary>
/// Request to create a <see cref="BalanceAdjustment"/>
/// </summary>
/// <remarks>
/// The Amount is a signed change to the Account's balance as it is displayed.
/// </remarks>
public record CreateBalanceAdjustmentRequest : CreateTransactionRequest
{
    /// <summary>
    /// Account whose balance is adjusted
    /// </summary>
    public required Account Account { get; init; }
}
