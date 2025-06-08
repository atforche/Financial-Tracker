using System.Diagnostics.CodeAnalysis;
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
    public bool IsValidToApplyToBalance(AccountBalance currentBalance, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (currentBalance.Account.Id != AccountId)
        {
            // Balance Event doesn't affect the provided balance
            return true;
        }
        FundAmount? fundAmount = currentBalance.FundBalances.SingleOrDefault(fundAmount => fundAmount.FundId == FromFundId);
        FundAmount? pendingFundAmount = currentBalance.PendingFundBalanceChanges.SingleOrDefault(fundAmount => fundAmount.FundId == FromFundId);
        if (fundAmount == null || Math.Min(fundAmount.Amount, fundAmount.Amount + (pendingFundAmount?.Amount ?? 0)) < Amount)
        {
            // Cannot apply this Balance Event if there is an insufficient amount for this Fund in this Account.
            // For simplicity, count pending balance decreases but don't count pending balance increases.
            exception = new InvalidOperationException();
        }
        return exception == null;
    }

    /// <inheritdoc/>
    public AccountBalance ApplyEventToBalance(AccountBalance currentBalance, ApplicationDirection direction)
    {
        if (AccountId != currentBalance.Account.Id)
        {
            return currentBalance;
        }
        currentBalance = currentBalance.AddNewAmount(new FundAmount
        {
            FundId = FromFundId,
            Amount = direction == ApplicationDirection.Reverse ? Amount : Amount * -1
        });
        currentBalance = currentBalance.AddNewAmount(new FundAmount
        {
            FundId = ToFundId,
            Amount = direction == ApplicationDirection.Reverse ? Amount * -1 : Amount
        });
        return currentBalance;
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
}