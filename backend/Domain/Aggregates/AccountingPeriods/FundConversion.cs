using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.ValueObjects;

namespace Domain.Aggregates.AccountingPeriods;

/// <summary>
/// Entity class representing a Fund Conversion
/// </summary>
/// <remarks>
/// A Fund Conversion represents an event where some amount from one Fund within an Account
/// gets converted into an amount from a different Fund. A Fund Conversion is instantenous and
/// this represents the only way to transfer money directly between Funds.
/// </remarks>
public class FundConversion : BalanceEventBase
{
    private AccountingPeriod _accountingPeriod;

    /// <summary>
    /// Parent Accounting Period for this Fund Converseion
    /// </summary>
    public override AccountingPeriod AccountingPeriod => _accountingPeriod;

    /// <summary>
    /// Fund that the amount is being converted out of for this Fund Conversion
    /// </summary>
    public Fund FromFund { get; private set; }

    /// <summary>
    /// Fund that the amount is being converted in to for this Fund Conversion
    /// </summary>
    public Fund ToFund { get; private set; }

    /// <summary>
    /// Amount for this Fund Conversion
    /// </summary>
    public decimal Amount { get; private set; }

    /// <inheritdoc/>
    public override AccountBalance ApplyEventToBalance(AccountBalance currentBalance) =>
        ApplyEventPrivate(currentBalance, false);

    /// <inheritdoc/>
    public override AccountBalance ReverseEventFromBalance(AccountBalance currentBalance) =>
        ApplyEventPrivate(currentBalance, true);

    /// <inheritdoc/>
    public override bool CanBeAppliedToBalance(AccountBalance currentBalance)
    {
        if (!base.CanBeAppliedToBalance(currentBalance))
        {
            return false;
        }
        // Cannot apply this Balance Event if there is an insufficient amount for this Fund in this Account.
        // For simplicity, count pending balance decreases but don't cound pending balance increases.
        FundAmount? fundAmount = currentBalance.FundBalances.SingleOrDefault(fundAmount => fundAmount.Fund == FromFund);
        FundAmount? pendingFundAmount = currentBalance.PendingFundBalanceChanges.SingleOrDefault(fundAmount => fundAmount.Fund == FromFund);
        if (fundAmount == null || Math.Min(fundAmount.Amount, fundAmount.Amount + (pendingFundAmount?.Amount ?? 0)) < Amount)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriod">Parent Accounting Period for this Fund Conversion</param>
    /// <param name="accountInfo">Account for this Fund Conversion</param>
    /// <param name="eventDate">Event Date for this Fund Conversion</param>
    /// <param name="fromFund">From Fund for this Fund Conversion</param>
    /// <param name="toFund">To Fund for this Fund Conversion</param>
    /// <param name="amount">Amount for this Fund Conversion</param>
    internal FundConversion(AccountingPeriod accountingPeriod,
        CreateBalanceEventAccountInfo accountInfo,
        DateOnly eventDate,
        Fund fromFund,
        Fund toFund,
        decimal amount)
        : base(accountInfo, eventDate, accountingPeriod.GetNextEventSequenceForDate(eventDate))
    {
        _accountingPeriod = accountingPeriod;
        FromFund = fromFund;
        ToFund = toFund;
        Amount = amount;
        Validate(accountInfo);
    }

    /// <inheritdoc/>
    protected override void Validate(CreateBalanceEventAccountInfo accountInfo)
    {
        base.Validate(accountInfo);
        if (FromFund == ToFund)
        {
            throw new InvalidOperationException();
        }
        if (Amount <= 0)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private FundConversion()
        : base()
    {
        _accountingPeriod = null!;
        FromFund = null!;
        ToFund = null!;
    }

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
        int fromFundFactor = isReverse ? 1 : -1;
        int toFundFactor = isReverse ? -1 : 1;
        Dictionary<Fund, decimal> fundBalances = currentBalance.FundBalances
            .ToDictionary(fundAmount => fundAmount.Fund, fundAmount => fundAmount.Amount);
        if (!fundBalances.TryAdd(FromFund, fromFundFactor * Amount))
        {
            fundBalances[FromFund] = fundBalances[FromFund] + fromFundFactor * Amount;
        }
        if (!fundBalances.TryAdd(ToFund, toFundFactor * Amount))
        {
            fundBalances[ToFund] = fundBalances[ToFund] + toFundFactor * Amount;
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