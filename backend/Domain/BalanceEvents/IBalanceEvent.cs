using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Domain.BalanceEvents;

/// <summary>
/// Interface representing a Balance Event
/// </summary>
public interface IBalanceEvent
{
    /// <summary>
    /// Accounting Period ID for this Balance Event
    /// </summary>
    AccountingPeriodId AccountingPeriodId { get; }

    /// <summary>
    /// Event Date for this Balance Event
    /// </summary>
    DateOnly EventDate { get; }

    /// <summary>
    /// Event Sequence for this Balance Event
    /// </summary>
    int EventSequence { get; }

    /// <summary>
    /// Account ID for this Balance Event
    /// </summary>
    AccountId AccountId { get; }

    /// <summary>
    /// Applies this Balance Event to the current balance of an Account
    /// </summary>
    /// <param name="currentBalance">Current Account Balance for an Account</param>
    /// <returns>The new balance of the Account after the Balance Event has been applied</returns>
    AccountBalance ApplyEventToBalance(AccountBalance currentBalance);

    /// <summary>
    /// Reverses the effects of this Balance Event from the current balance of an Account
    /// </summary>
    /// <param name="currentBalance">Current Account Balance for an Account</param>
    /// <returns>The new balance of the Account after the Balance Event has been reversed</returns>
    AccountBalance ReverseEventFromBalance(AccountBalance currentBalance);

    /// <summary>
    /// Determines if this Balance Event can be applied to the current balance 
    /// </summary>
    /// <param name="currentBalance">Current Account Balance for an Account</param>
    /// <returns>True if this Balance Event can be applied to the current balance, false otherwise</returns>
    bool CanBeAppliedToBalance(AccountBalance currentBalance);

    /// <summary>
    /// Determines if Balance Event falls later than the provided Balance Event
    /// </summary>
    /// <param name="otherBalanceEvent">Other Balance Event to compare to</param>
    /// <returns>True if this Balance Event falls later than the provided Balance Event, false otherwise</returns>
    bool IsLaterThan(IBalanceEvent otherBalanceEvent) => new BalanceEventComparer().Compare(this, otherBalanceEvent) > 0;
}