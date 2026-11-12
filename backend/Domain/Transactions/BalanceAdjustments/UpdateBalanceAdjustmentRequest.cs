using Domain.Accounts;

namespace Domain.Transactions.BalanceAdjustments;

/// <summary>
/// Request to update a <see cref="BalanceAdjustment"/>
/// </summary>
/// <remarks>
/// The Amount is a signed change to the Account's balance as it is displayed.
/// </remarks>
public record UpdateBalanceAdjustmentRequest : UpdateTransactionRequest
{
    /// <summary>
    /// Account whose balance is adjusted
    /// </summary>
    public required Account Account { get; init; }
}
