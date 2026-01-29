using System.Diagnostics.CodeAnalysis;
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
    public AccountId AccountId { get; }

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
    /// Attempts to construct a new instance of this class
    /// </summary>
    internal static bool TryCreate(
        AccountId accountId,
        IEnumerable<FundAmount> fundBalances,
        IEnumerable<FundAmount> pendingFundBalanceChanges,
        out AccountBalance? accountBalance,
        out IEnumerable<Exception> exceptions)
    {
        accountBalance = null;
        exceptions = [];

        fundBalances = fundBalances
            .GroupBy(fundAmount => fundAmount.FundId)
            .Select(group => new FundAmount
            {
                FundId = group.Key,
                Amount = group.Sum(fundAmount => fundAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
        pendingFundBalanceChanges = pendingFundBalanceChanges
            .GroupBy(fundAmount => fundAmount.FundId)
            .Select(group => new FundAmount
            {
                FundId = group.Key,
                Amount = group.Sum(fundAmount => fundAmount.Amount)
            })
            .Where(amount => amount.Amount != 0.00m)
            .ToList();
        if (fundBalances.Sum(balance => balance.Amount) < 0 ||
            fundBalances.Sum(balance => balance.Amount) + pendingFundBalanceChanges.Sum(balance => balance.Amount) < 0)
        {
            exceptions = exceptions.Append(new InvalidOperationException("Account balance cannot be negative."));
            return false;
        }
        accountBalance = new AccountBalance(accountId, fundBalances, pendingFundBalanceChanges);
        return true;
    }

    /// <summary>
    /// Attempts to add the provided Fund Amounts to the current Account Balance
    /// </summary>
    internal bool TryAddNewAmounts(IEnumerable<FundAmount> fundAmounts, [NotNullWhen(true)] out AccountBalance? newAccountBalance, out IEnumerable<Exception> exceptions) =>
        TryCreate(AccountId, FundBalances.Concat(fundAmounts), PendingFundBalanceChanges, out newAccountBalance, out exceptions);

    /// <summary>
    /// Attempts to add the provided Fund Amounts to the current pending Account Balance
    /// </summary>
    internal bool TryAddNewPendingAmounts(IEnumerable<FundAmount> fundAmounts, [NotNullWhen(true)] out AccountBalance? newAccountBalance, out IEnumerable<Exception> exceptions) =>
        TryCreate(AccountId, FundBalances, PendingFundBalanceChanges.Concat(fundAmounts), out newAccountBalance, out exceptions);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountBalance(AccountId accountId, IEnumerable<FundAmount> fundBalances, IEnumerable<FundAmount> pendingFundBalanceChanges)
    {
        AccountId = accountId;
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
    }
}