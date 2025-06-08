using System.Diagnostics.CodeAnalysis;
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
    /// Gets the Account IDs associated with this Balance Event
    /// </summary>
    IReadOnlyCollection<AccountId> GetAccountIds();

    /// <summary>
    /// Determines if this Balance Event can be applied to the current balance 
    /// </summary>
    /// <param name="currentBalance">Current Account Balance for an Account</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if this Balance Event can be applied to the current balance, false otherwise</returns>
    bool IsValidToApplyToBalance(AccountBalance currentBalance, [NotNullWhen(false)] out Exception? exception);

    /// <summary>
    /// Applies this Balance Event to the current balance of an Account
    /// </summary>
    /// <param name="currentBalance">Current Account Balance for an Account</param>
    /// <param name="direction">Direction in which to apply this Balance Event</param>
    /// <returns>The new balance of the Account after the Balance Event has been applied</returns>
    AccountBalance ApplyEventToBalance(AccountBalance currentBalance, ApplicationDirection direction);

    /// <summary>
    /// Determines if this Balance Event falls later than the provided Balance Event
    /// </summary>
    /// <param name="otherBalanceEvent">Other Balance Event to compare to</param>
    /// <returns>True if this Balance Event falls later than the provided Balance Event, false otherwise</returns>
    bool IsLaterThan(IBalanceEvent otherBalanceEvent) => new BalanceEventComparer().Compare(this, otherBalanceEvent) > 0;
}

/// <summary>
/// Enum representing the different direction of applying a Balance Event to a balance
/// </summary>
public enum ApplicationDirection
{
    /// <summary>
    /// Applies the effects of this Balance Event as normal
    /// </summary>
    Standard,

    /// <summary>
    /// Reverses the effects of this Balance Event
    /// </summary>
    Reverse
}