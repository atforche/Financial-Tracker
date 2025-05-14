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
    public AccountBalance(Account account, IEnumerable<FundAmount> fundBalances, IEnumerable<FundAmount> pendingFundBalanceChanges)
    {
        Account = account;
        FundBalances = fundBalances
            .Where(amount => amount.Amount != 0.00m)
            .GroupBy(fundAmount => fundAmount.Fund)
            .Select(group => new FundAmount
            {
                Fund = group.Key,
                Amount = group.Sum(fundAmount => fundAmount.Amount)
            })
            .ToList();
        PendingFundBalanceChanges = pendingFundBalanceChanges
            .Where(amount => amount.Amount != 0.00m)
            .GroupBy(fundAmount => fundAmount.Fund)
            .Select(group => new FundAmount
            {
                Fund = group.Key,
                Amount = group.Sum(fundAmount => fundAmount.Amount)
            })
            .ToList();
        Validate();
    }

    /// <summary>
    /// Validates this Account Balance
    /// </summary>
    private void Validate()
    {
        if (Balance < 0 || BalanceIncludingPending < 0)
        {
            throw new InvalidOperationException();
        }
        if (FundBalances.GroupBy(fundBalance => fundBalance.Fund).Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException();
        }
        if (PendingFundBalanceChanges.GroupBy(fundBalance => fundBalance.Fund).Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException();
        }
    }
}