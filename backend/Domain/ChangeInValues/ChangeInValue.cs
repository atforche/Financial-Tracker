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
    public AccountBalance ApplyEventToBalance(AccountBalance currentBalance) =>
        ApplyEventPrivate(currentBalance, false);

    /// <inheritdoc/>
    public AccountBalance ReverseEventFromBalance(AccountBalance currentBalance) =>
        ApplyEventPrivate(currentBalance, false);

    /// <inheritdoc/>
    public bool CanBeAppliedToBalance(AccountBalance currentBalance)
    {
        if (AccountId != currentBalance.Account.Id)
        {
            return true;
        }
        // Change In Values that are increasing an Accounts balance are always valid
        if (FundAmount.Amount > 0)
        {
            return true;
        }
        // Cannot apply this Balance Event if it will take the Account's overall balance negative
        // For simplicity, count pending balance decreases but don't count pending balance increases.
        return Math.Min(currentBalance.Balance, currentBalance.BalanceIncludingPending) + FundAmount.Amount >= 0;
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
        int balanceChangeFactor = isReverse ? -1 : 1;
        var fundBalances = currentBalance.FundBalances.ToDictionary(fundAmount => fundAmount.FundId, fundAmount => fundAmount.Amount);
        if (!fundBalances.TryAdd(FundAmount.FundId, balanceChangeFactor * FundAmount.Amount))
        {
            fundBalances[FundAmount.FundId] = fundBalances[FundAmount.FundId] + (balanceChangeFactor * FundAmount.Amount);
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