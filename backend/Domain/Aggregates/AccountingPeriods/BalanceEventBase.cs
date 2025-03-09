using Domain.Aggregates.Accounts;
using Domain.ValueObjects;

namespace Domain.Aggregates.AccountingPeriods;

/// <summary>
/// Base class shared by all entities that impact the balance of an Account
/// </summary>
public abstract class BalanceEventBase : EntityBase
{
    /// <summary>
    /// Accounting Period for this Balance Event
    /// </summary>
    public abstract AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Account for this Balance Event
    /// </summary>
    public Account Account { get; private set; }

    /// <summary>
    /// Date for this Balance Event
    /// </summary>
    public DateOnly EventDate { get; private set; }

    /// <summary>
    /// Event Sequence for this Balance Event
    /// </summary>
    /// <remarks>
    /// This sequence number provides a strong ordering to all the Balance Events that
    /// take place on the same date
    /// </remarks>
    public int EventSequence { get; private set; }

    /// <summary>
    /// Applies this Balance Event to the current balance of an Account
    /// </summary>
    /// <param name="currentBalance">Current Account Balance for an Account</param>
    /// <returns>The new balance of the Account after the Balance Event has been applied</returns>
    public abstract AccountBalance ApplyEventToBalance(AccountBalance currentBalance);

    /// <summary>
    /// Reverses the effects of this Balance Event from the current balance of an Account
    /// </summary>
    /// <param name="currentBalance">Current Account Balance for an Account</param>
    /// <returns>The new balance of the Account after the Balance Event has been reversed</returns>
    public abstract AccountBalance ReverseEventFromBalance(AccountBalance currentBalance);

    /// <summary>
    /// Determines if this Balance Event can be applied to the current balance 
    /// </summary>
    /// <param name="currentBalance">Current Account Balance for an Account</param>
    /// <returns>True if this Balance Event can be applied to the current balance, false otherwise</returns>
    public virtual bool CanBeAppliedToBalance(AccountBalance currentBalance)
    {
        if (Account != currentBalance.Account)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountInfo">Create Balance Event Account Info for this Balance Event</param>
    /// <param name="eventDate">Event Date for this Balance Event</param>
    /// <param name="eventSequence"></param>
    protected BalanceEventBase(CreateBalanceEventAccountInfo accountInfo, DateOnly eventDate, int eventSequence)
        : base(new EntityId(default, Guid.NewGuid()))
    {
        Account = accountInfo.Account;
        EventDate = eventDate;
        EventSequence = eventSequence;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected BalanceEventBase() : base(new EntityId(default, Guid.NewGuid())) => Account = null!;

    /// <summary>
    /// Validates the current Balance Event
    /// </summary>
    /// <param name="accountInfo">Create Balance Event Account Info for this Balance Event</param>
    protected virtual void Validate(CreateBalanceEventAccountInfo accountInfo)
    {
        if (!AccountingPeriod.IsOpen)
        {
            throw new InvalidOperationException();
        }
        if (EventDate == DateOnly.MinValue)
        {
            throw new InvalidOperationException();
        }
        if (EventSequence <= 0)
        {
            throw new InvalidOperationException();
        }
        // Validate that there are no duplicate event sequences for this date
        if (AccountingPeriod.GetAllBalanceEvents()
                .Any(balanceEvent => balanceEvent.EventDate == EventDate && balanceEvent.EventSequence == EventSequence))
        {
            throw new InvalidOperationException();
        }
        // Validate that a balance event can only be added with a date in a month adjacent to the Accounting Period month
        int monthDifference = (AccountingPeriod.Year - EventDate.Year) * 12 + AccountingPeriod.Month - EventDate.Month;
        if (Math.Abs(monthDifference) > 1)
        {
            throw new InvalidOperationException();
        }
        // Validate that this Balance Event is valid to be applied to the current Account Balance
        if (!CanBeAppliedToBalance(accountInfo.CurrentBalance))
        {
            throw new InvalidOperationException();
        }
        // Validate that adding this Balance Event doesn't cause any of the future Balance Events to become invalid
        AccountBalance runningBalance = ApplyEventToBalance(accountInfo.CurrentBalance);
        foreach (BalanceEventBase balanceEvent in accountInfo.FutureBalanceEventsForAccount)
        {
            if (!balanceEvent.CanBeAppliedToBalance(runningBalance))
            {
                throw new InvalidOperationException();
            }
            runningBalance = balanceEvent.ApplyEventToBalance(runningBalance);
        }
        // Validate that adding this Balance Event doesn't cause the balance of this account within the
        // Accounting Period to become invalid
        runningBalance = ApplyEventToBalance(accountInfo.CurrentBalance);
        foreach (BalanceEventBase balanceEvent in accountInfo.FutureBalanceEventsForAccount
            .Where(balanceEvent => balanceEvent.AccountingPeriod == AccountingPeriod))
        {
            if (!balanceEvent.CanBeAppliedToBalance(runningBalance))
            {
                throw new InvalidOperationException();
            }
            runningBalance = balanceEvent.ApplyEventToBalance(runningBalance);
        }
    }
}

/// <summary>
/// Account information needed to construct an Account Balance
/// </summary>
public class CreateBalanceEventAccountInfo
{
    /// <summary>
    /// Account for this Balance Event
    /// </summary>
    public Account Account { get; init; }

    /// <summary>
    /// Current Balance for the Account for this Balance Event
    /// </summary>
    public AccountBalance CurrentBalance { get; init; }

    /// <summary>
    /// Future Balance Events for the Account for this Balance Event
    /// </summary>
    public ICollection<BalanceEventBase> FutureBalanceEventsForAccount { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="account">Account for this Balance Event</param>
    /// <param name="currentBalance">Current Balance for the Account for this Balance Event</param>
    /// <param name="futureBalanceEventsForAccount">Future Balance Events for the Account for this Balance Even</param>
    public CreateBalanceEventAccountInfo(Account account,
        AccountBalance currentBalance,
        IEnumerable<BalanceEventBase> futureBalanceEventsForAccount)
    {
        Account = account;
        CurrentBalance = currentBalance;
        FutureBalanceEventsForAccount = futureBalanceEventsForAccount.ToList();
        Validate();
    }

    /// <summary>
    /// Validates the current Create Balance Event Account Info
    /// </summary>
    private void Validate()
    {
        if (Account != CurrentBalance.Account)
        {
            throw new InvalidOperationException();
        }
        if (FutureBalanceEventsForAccount.Any(balanceEvent => balanceEvent.Account != Account))
        {
            throw new InvalidOperationException();
        }
    }
}