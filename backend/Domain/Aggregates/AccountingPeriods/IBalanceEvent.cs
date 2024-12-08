using Domain.Aggregates.Accounts;
using Domain.ValueObjects;

namespace Domain.Aggregates.AccountingPeriods;

/// <summary>
/// Interface shared by all entities that impact the balance of an Account
/// </summary>
public interface IBalanceEvent
{
    /// <summary>
    /// Account for this Balance Event
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Accounting Period for this Balance Event
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Date for this Balance Event
    /// </summary>
    public DateOnly EventDate { get; }

    /// <summary>
    /// Event Sequence for this Balance Event
    /// </summary>
    /// <remarks>
    /// This sequence number provides a strong ordering to all the Balance Events that
    /// take place on the same date
    /// </remarks>
    public int EventSequence { get; }

    /// <summary>
    /// Applies this Balance Event to the current balance of an Account
    /// </summary>
    /// <param name="currentBalance">Current Account Balance for an Account</param>
    /// <returns>The new balance of the Account after the Balance Event has been applied</returns>
    public AccountBalance ApplyEventToBalance(AccountBalance currentBalance);

    /// <summary>
    /// Reverses the effects of this Balance Event from the current balance of an Account
    /// </summary>
    /// <param name="currentBalance">Current Account Balance for an Account</param>
    /// <returns>The new balance of the Account after the Balane Event has been reversed</returns>
    public AccountBalance ReverseEventFromBalance(AccountBalance currentBalance);
}