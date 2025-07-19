using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.BalanceEvents;
using Domain.Funds;

namespace Domain.ChangeInValues;

/// <summary>
/// Entity class representing a Change in Value
/// </summary>
/// <remarks>
/// A Change In Value represents an event where the balance of an Account is changed however this event isn't
/// represented as a single distinct Transaction on an account statement. Examples of this include interest
/// that is constantly accruing on a loan or changes in stock market value for an investment Account.
/// </remarks>
public class ChangeInValue : Entity<ChangeInValueId>, IBalanceEvent
{
    /// <inheritdoc/>
    public AccountingPeriodId AccountingPeriodId { get; private set; }

    /// <inheritdoc/>
    public DateOnly EventDate { get; private set; }

    /// <inheritdoc/>
    public int EventSequence { get; private set; }

    /// <summary>
    /// Account ID for this Change In Value
    /// </summary>
    public AccountId AccountId { get; private set; }

    /// <summary>
    /// Fund Amount for this Change In Value
    /// </summary>
    public FundAmount FundAmount { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountId> GetAccountIds() => [AccountId];

    /// <inheritdoc/>
    public bool IsValidToApplyToBalance(AccountBalance currentBalance, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (AccountId != currentBalance.Account.Id)
        {
            // Balance Event doesn't affect the provided balance
            return true;
        }
        if (FundAmount.Amount > 0)
        {
            // Change In Values that are increasing an Accounts balance are always valid
            return true;
        }
        if (Math.Min(currentBalance.Balance, currentBalance.BalanceIncludingPending) + FundAmount.Amount < 0)
        {
            // Cannot apply this Balance Event if it will take the Account's overall balance negative
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
        return currentBalance.AddNewAmount(direction == ApplicationDirection.Reverse ? FundAmount.GetWithReversedAmount() : FundAmount);
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriodId">Accounting Period ID for this Change In Value</param>
    /// <param name="eventDate">Event Date for this Change In Value</param>
    /// <param name="eventSequence">Event Sequence for this Change In Value</param>
    /// <param name="accountId">Account ID for this Change In Value</param>
    /// <param name="fundAmount">Fund Amount for this Change In Value</param>
    internal ChangeInValue(AccountingPeriodId accountingPeriodId,
        DateOnly eventDate,
        int eventSequence,
        AccountId accountId,
        FundAmount fundAmount)
        : base(new ChangeInValueId(Guid.NewGuid()))
    {
        AccountingPeriodId = accountingPeriodId;
        EventDate = eventDate;
        EventSequence = eventSequence;
        AccountId = accountId;
        FundAmount = fundAmount;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private ChangeInValue() : base()
    {
        AccountingPeriodId = null!;
        AccountId = null!;
        FundAmount = null!;
    }
}