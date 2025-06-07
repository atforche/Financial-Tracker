using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.Funds;

namespace Domain.FundConversions;

/// <summary>
/// Entity class representing a Fund Conversion
/// </summary>
/// <remarks>
/// A Fund Conversion represents an event where some amount from one Fund within an Account
/// gets converted into an amount from a different Fund. A Fund Conversion is instantaneous and
/// this represents the only way to transfer money directly between Funds.
/// </remarks>
public class FundConversion : Entity<FundConversionId>, IBalanceEvent
{
    /// <inheritdoc/>
    public AccountingPeriodId AccountingPeriodId { get; private set; }

    /// <inheritdoc/>
    public DateOnly EventDate { get; private set; }

    /// <inheritdoc/>
    public int EventSequence { get; private set; }

    /// <summary>
    /// Account ID for this Fund Conversion
    /// </summary>
    public AccountId AccountId { get; private set; }

    /// <summary>
    /// Fund that the amount is being converted out of for this Fund Conversion
    /// </summary>
    public FundId FromFundId { get; private set; }

    /// <summary>
    /// Fund that the amount is being converted in to for this Fund Conversion
    /// </summary>
    public FundId ToFundId { get; private set; }

    /// <summary>
    /// Amount for this Fund Conversion
    /// </summary>
    public decimal Amount { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountId> GetAccountIds() => [AccountId];

    /// <inheritdoc/>
    public AccountBalance ApplyEventToBalance(AccountBalance currentBalance) => ApplyEventPrivate(currentBalance, false);

    /// <inheritdoc/>
    public AccountBalance ReverseEventFromBalance(AccountBalance currentBalance) => ApplyEventPrivate(currentBalance, true);

    /// <inheritdoc/>
    public bool CanBeAppliedToBalance(AccountBalance currentBalance)
    {
        if (currentBalance.Account.Id != AccountId)
        {
            return true;
        }
        // Cannot apply this Balance Event if there is an insufficient amount for this Fund in this Account.
        // For simplicity, count pending balance decreases but don't count pending balance increases.
        FundAmount? fundAmount = currentBalance.FundBalances.SingleOrDefault(fundAmount => fundAmount.FundId == FromFundId);
        FundAmount? pendingFundAmount = currentBalance.PendingFundBalanceChanges.SingleOrDefault(fundAmount => fundAmount.FundId == FromFundId);
        if (fundAmount == null || Math.Min(fundAmount.Amount, fundAmount.Amount + (pendingFundAmount?.Amount ?? 0)) < Amount)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriodId">Accounting Period ID for this Fund Conversion</param>
    /// <param name="eventDate">Event Date for this Fund Conversion</param>
    /// <param name="eventSequence">Event Sequence for this Fund Conversion</param>
    /// <param name="accountId">Account ID for this Fund Conversion</param>
    /// <param name="fromFundId">From Fund ID for this Fund Conversion</param>
    /// <param name="toFundId">To Fund ID for this Fund Conversion</param>
    /// <param name="amount">Amount for this Fund Conversion</param>
    internal FundConversion(AccountingPeriodId accountingPeriodId,
        DateOnly eventDate,
        int eventSequence,
        AccountId accountId,
        FundId fromFundId,
        FundId toFundId,
        decimal amount)
        : base(new FundConversionId(Guid.NewGuid()))
    {
        AccountingPeriodId = accountingPeriodId;
        EventDate = eventDate;
        EventSequence = eventSequence;
        AccountId = accountId;
        FromFundId = fromFundId;
        ToFundId = toFundId;
        Amount = amount;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private FundConversion()
        : base()
    {
        AccountingPeriodId = null!;
        AccountId = null!;
        FromFundId = null!;
        ToFundId = null!;
    }

    /// <summary>
    /// Applies the Fund Conversion to the provided balance
    /// </summary>
    /// <param name="currentBalance">Account Balance to apply this event to</param>
    /// <param name="isReverse">True if this Fund Conversion is being reversed, false otherwise</param>
    /// <returns>The new Account Balance after this event has been applied</returns>
    private AccountBalance ApplyEventPrivate(AccountBalance currentBalance, bool isReverse)
    {
        if (AccountId != currentBalance.Account.Id)
        {
            return currentBalance;
        }
        int fromFundFactor = isReverse ? 1 : -1;
        int toFundFactor = isReverse ? -1 : 1;
        var fundBalances = currentBalance.FundBalances.ToDictionary(fundAmount => fundAmount.FundId, fundAmount => fundAmount.Amount);
        if (!fundBalances.TryAdd(FromFundId, fromFundFactor * Amount))
        {
            fundBalances[FromFundId] = fundBalances[FromFundId] + (fromFundFactor * Amount);
        }
        if (!fundBalances.TryAdd(ToFundId, toFundFactor * Amount))
        {
            fundBalances[ToFundId] = fundBalances[ToFundId] + (toFundFactor * Amount);
        }
        return new AccountBalance(currentBalance.Account,
            fundBalances.Select(pair => new FundAmount
            {
                FundId = pair.Key,
                Amount = pair.Value
            }),
            currentBalance.PendingFundBalanceChanges);
    }
}