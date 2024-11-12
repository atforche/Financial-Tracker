namespace Domain.ValueObjects;

/// <summary>
/// Value object class representing the balance of an Account
/// </summary>
public class AccountBalance
{
    /// <summary>
    /// Current balance for each Fund within the Account
    /// </summary>
    public required IReadOnlyCollection<FundAmount> FundBalances { get; init; }

    /// <summary>
    /// Change in each Fund balance that is currently pending within the Account
    /// </summary>
    public required IReadOnlyCollection<FundAmount> PendingFundBalanceChanges { get; init; }

    /// <summary>
    /// Balance of the Account
    /// </summary>
    public decimal Balance => FundBalances.Sum(balance => balance.Amount);

    /// <summary>
    /// Total pending balance change for this Account
    /// </summary>
    public decimal TotalPendingBalanceChange => PendingFundBalanceChanges.Sum(balance => balance.Amount);
}