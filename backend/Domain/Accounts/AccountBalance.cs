using Domain.Funds;

namespace Domain.Accounts;

/// <summary>
/// Value object class representing the balance of an Account
/// </summary>
public class AccountBalance
{
    /// <summary>
    /// Account for this Account Balance
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Fund Balances for this Account Balance
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundBalances { get; }

    /// <summary>
    /// Pending Fund Balance Changes for this Account Balance
    /// </summary>
    public IReadOnlyCollection<FundAmount> PendingFundBalanceChanges { get; }

    /// <summary>
    /// Balance for this Account Balance
    /// </summary>
    public decimal Balance => FundBalances.Sum(balance => balance.Amount);

    /// <summary>
    /// Total Pending Balance Change for this Account Balance
    /// </summary>
    public decimal TotalPendingBalanceChange => PendingFundBalanceChanges.Sum(balance => balance.Amount);

    /// <summary>
    /// Balance including any pending changes for this Account Balance
    /// </summary>
    public decimal BalanceIncludingPending => Balance + TotalPendingBalanceChange;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="account">Account for this Account Balance</param>
    /// <param name="fundBalances">Fund Balances for this Account Balance</param>
    /// <param name="pendingFundBalanceChanges">Pending Fund Balance Changes for this Account Balance</param>
    internal AccountBalance(Account account, IEnumerable<FundAmount> fundBalances, IEnumerable<FundAmount> pendingFundBalanceChanges)
    {
        Account = account;
        FundBalances = fundBalances
            .GroupBy(fundAmount => fundAmount.FundId)
            .Select(group => new FundAmount
            {
                FundId = group.Key,
                Amount = group.Sum(fundAmount => fundAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
        PendingFundBalanceChanges = pendingFundBalanceChanges
            .GroupBy(fundAmount => fundAmount.FundId)
            .Select(group => new FundAmount
            {
                FundId = group.Key,
                Amount = group.Sum(fundAmount => fundAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
        Validate();
    }

    /// <summary>
    /// Adds the provided Fund Amount to the current Account Balance
    /// </summary>
    /// <param name="fundAmount">Fund Amount to add to the current balance</param>
    /// <returns>The new Account Balance</returns>
    internal AccountBalance AddNewAmount(FundAmount fundAmount) => new(Account, FundBalances.Concat([fundAmount]), PendingFundBalanceChanges);

    /// <summary>
    /// Adds the provided Fund Amount to the current pending Account Balance
    /// </summary>
    /// <param name="fundAmount">Fund Amount to add to the current pending balance</param>
    /// <returns>The new Account Balance</returns>
    internal AccountBalance AddNewPendingAmount(FundAmount fundAmount) => new(Account, FundBalances, PendingFundBalanceChanges.Concat([fundAmount]));

    /// <summary>
    /// Validates this Account Balance
    /// </summary>
    private void Validate()
    {
        if (Balance < 0 || BalanceIncludingPending < 0)
        {
            throw new InvalidOperationException();
        }
        if (FundBalances.GroupBy(fundBalance => fundBalance.FundId).Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException();
        }
        if (PendingFundBalanceChanges.GroupBy(fundBalance => fundBalance.FundId).Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException();
        }
    }
}