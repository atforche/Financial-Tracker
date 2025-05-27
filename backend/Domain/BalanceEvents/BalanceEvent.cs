using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Domain.BalanceEvents;

/// <summary>
/// Base class shared by all entities that impact the balance of an Account
/// </summary>
public abstract class BalanceEvent : EntityOld, IComparable<BalanceEvent>
{
    /// <summary>
    /// Accounting Period ID for this Balance Event
    /// </summary>
    public AccountingPeriodId AccountingPeriodId { get; private set; }

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
    /// Account for this Balance Event
    /// </summary>
    public Account Account { get; private set; }

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
    /// <param name="eventDate">Event Date for this Balance Event</param>
    /// <param name="eventSequence">Event Sequence for this Balance Event</param>
    /// <param name="account">Account for this Balance Event</param>
    protected BalanceEvent(AccountingPeriod accountingPeriod, DateOnly eventDate, int eventSequence, Account account)
    {
        AccountingPeriodId = accountingPeriod.Id;
        EventDate = eventDate;
        EventSequence = eventSequence;
        Account = account;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    protected BalanceEvent()
    {
        AccountingPeriodId = null!;
        Account = null!;
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
    public override int GetHashCode() => HashCode.Combine(AccountingPeriodId, EventDate, EventSequence);

    #endregion
}