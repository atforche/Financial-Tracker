using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.ValueObjects;

namespace Domain.Aggregates;

/// <summary>
/// Base class shared by all entities that impact the balance of an Account
/// </summary>
public abstract class BalanceEvent : Entity, IComparable<BalanceEvent>
{
    /// <summary>
    /// Accounting Period Year for this Balance Event
    /// </summary>
    public int AccountingPeriodYear { get; private set; }

    /// <summary>
    /// Accounting Period Month for this Balance Event
    /// </summary>
    public int AccountingPeriodMonth { get; private set; }

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
    /// <param name="accountingPeriod">Accounting Period for this Balance Event</param>
    /// <param name="account">Account for this Balance Event</param>
    /// <param name="eventDate">Event Date for this Balance Event</param>
    /// <param name="sequenceOffset">Sequence offset for this Balance Event</param>
    protected BalanceEvent(AccountingPeriod accountingPeriod, Account account, DateOnly eventDate, int sequenceOffset = 0)
        : base(new EntityId(default, Guid.NewGuid()))
    {
        AccountingPeriodYear = accountingPeriod.Year;
        AccountingPeriodMonth = accountingPeriod.Month;
        Account = account;
        EventDate = eventDate;
        EventSequence = GetNextEventSequenceForDate(accountingPeriod, eventDate) + sequenceOffset;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected BalanceEvent() : base(new EntityId(default, Guid.NewGuid())) => Account = null!;

    /// <summary>
    /// Gets the next Balance Event sequence for the provided date
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for the Balance Event</param>
    /// <param name="eventDate">Event date to get the next Balance Event sequence for</param>
    /// <returns>The next Balance Event sequence for the provided date</returns>
    private static int GetNextEventSequenceForDate(AccountingPeriod accountingPeriod, DateOnly eventDate)
    {
        var balanceEventsOnDate = accountingPeriod.GetAllBalanceEvents()
            .Where(balanceEvent => balanceEvent.EventDate == eventDate).ToList();
        if (balanceEventsOnDate.Count == 0)
        {
            return 1;
        }
        return balanceEventsOnDate.Count + 1;
    }

    #region IComparable

    /// <inheritdoc/>
    public int CompareTo(BalanceEvent? other)
    {
        if (other is null)
        {
            return 1;
        }
        if (EventDate != other.EventDate)
        {
            return EventDate.CompareTo(other.EventDate);
        }
        if (AccountingPeriodYear != other.AccountingPeriodYear)
        {
            return AccountingPeriodYear.CompareTo(other.AccountingPeriodYear);
        }
        if (AccountingPeriodMonth != other.AccountingPeriodMonth)
        {
            return AccountingPeriodMonth.CompareTo(other.AccountingPeriodMonth);
        }
        return EventSequence.CompareTo(other.EventSequence);
    }

    /// <inheritdoc/>
    public static bool operator ==(BalanceEvent operand1, BalanceEvent operand2) => operand1.CompareTo(operand2) == 0;

    /// <inheritdoc/>
    public static bool operator !=(BalanceEvent operand1, BalanceEvent operand2) => operand1.CompareTo(operand2) != 0;

    /// <inheritdoc/>
    public static bool operator >(BalanceEvent operand1, BalanceEvent operand2) => operand1.CompareTo(operand2) > 0;

    /// <inheritdoc/>
    public static bool operator <(BalanceEvent operand1, BalanceEvent operand2) => operand1.CompareTo(operand2) < 0;

    /// <inheritdoc/>
    public static bool operator >=(BalanceEvent operand1, BalanceEvent operand2) => operand1.CompareTo(operand2) >= 0;

    /// <inheritdoc/>
    public static bool operator <=(BalanceEvent operand1, BalanceEvent operand2) => operand1.CompareTo(operand2) <= 0;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is BalanceEvent otherBalanceEvent && CompareTo(otherBalanceEvent) == 0;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(AccountingPeriodYear, AccountingPeriodMonth, EventDate, EventSequence);

    #endregion
}