using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.Funds;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity class representing a Change in Value
/// </summary>
/// <remarks>
/// A Change In Value represents an event where the balance of an Account is changed however this event isn't
/// represented as a single distinct Transaction on an account statement. Examples of this include interest
/// that is constantly accruing on a loan or changes in stock market value for an investment Account.
/// </remarks>
public class ChangeInValue : BalanceEvent
{
    /// <summary>
    /// Accounting Entry for this Change In Value
    /// </summary>
    public FundAmount AccountingEntry { get; private set; }

    /// <inheritdoc/>
    public override AccountBalance ApplyEventToBalance(AccountBalance currentBalance) =>
        ApplyEventPrivate(currentBalance, false);

    /// <inheritdoc/>
    public override AccountBalance ReverseEventFromBalance(AccountBalance currentBalance) =>
        ApplyEventPrivate(currentBalance, false);

    /// <inheritdoc/>
    public override bool CanBeAppliedToBalance(AccountBalance currentBalance)
    {
        if (!base.CanBeAppliedToBalance(currentBalance))
        {
            return false;
        }
        // Change In Values that are increasing an Accounts balance are always valid
        if (AccountingEntry.Amount > 0)
        {
            return true;
        }
        // Cannot apply this Balance Event if it will take the Account's overall balance negative
        // For simplicity, count pending balance decreases but don't count pending balance increases.
        return Math.Min(currentBalance.Balance, currentBalance.BalanceIncludingPending) + AccountingEntry.Amount >= 0;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for this Change In Value</param>
    /// <param name="account">Account for this Change In Value</param>
    /// <param name="eventDate">Event Date for this Change In Value</param>
    /// <param name="accountingEntry">Accounting Entry for this Change In Value</param>
    internal ChangeInValue(AccountingPeriod accountingPeriod,
        Account account,
        DateOnly eventDate,
        FundAmount accountingEntry)
        : base(accountingPeriod, account, eventDate) =>
        AccountingEntry = accountingEntry;

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private ChangeInValue()
        : base() =>
        AccountingEntry = null!;

    /// <summary>
    /// Applies the Fund Conversion to the provided balance
    /// </summary>
    /// <param name="currentBalance">Account Balance to apply this event to</param>
    /// <param name="isReverse">True if this Fund Conversion is being reversed, false otherwise</param>
    /// <returns>The new Account Balance after this event has been applied</returns>
    private AccountBalance ApplyEventPrivate(AccountBalance currentBalance, bool isReverse)
    {
        if (Account.Id != currentBalance.Account.Id)
        {
            return currentBalance;
        }
        int balanceChangeFactor = isReverse ? -1 : 1;
        var fundBalances = currentBalance.FundBalances.ToDictionary(fundAmount => fundAmount.Fund, fundAmount => fundAmount.Amount);
        if (!fundBalances.TryAdd(AccountingEntry.Fund, balanceChangeFactor * AccountingEntry.Amount))
        {
            fundBalances[AccountingEntry.Fund] = fundBalances[AccountingEntry.Fund] + (balanceChangeFactor * AccountingEntry.Amount);
        }
        return new AccountBalance(currentBalance.Account,
            fundBalances.Select(pair => new FundAmount
            {
                Fund = pair.Key,
                Amount = pair.Value
            }),
            currentBalance.PendingFundBalanceChanges);
    }
}