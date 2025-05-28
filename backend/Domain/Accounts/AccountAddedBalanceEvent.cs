using Domain.AccountingPeriods;
using Domain.BalanceEvents;
using Domain.Funds;

namespace Domain.Accounts;

/// <summary>
/// Entity class representing an Account Added Balance Event
/// </summary>
/// <remarks>
/// An Account Added Balance Event represents the very first Balance Event for an Account after it is added.
/// The Account cannot have any Balance Events that fall on earlier dates or in earlier Accounting Periods.
/// </remarks>
public class AccountAddedBalanceEvent : BalanceEvent
{
    /// <summary>
    /// Fund Amounts for this Account Added Balance Event
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundAmounts { get; private set; }

    /// <inheritdoc/>
    public override AccountBalance ApplyEventToBalance(AccountBalance currentBalance) => new(Account, FundAmounts, []);

    /// <inheritdoc/>
    public override AccountBalance ReverseEventFromBalance(AccountBalance currentBalance) => new(Account, [], []);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriodId">Accounting Period ID for this Account Added Balance Event</param>
    /// <param name="eventDate">Event Date for this Account Added Balance Event</param>
    /// <param name="eventSequence">Event Sequence for this Account Added Balance Event</param>
    /// <param name="account">Account for this Account Added Balance Event</param>
    /// <param name="startingFundBalances">Starting Fund Balances for this Account Added Balance Event</param>
    internal AccountAddedBalanceEvent(
        AccountingPeriodId accountingPeriodId,
        DateOnly eventDate,
        int eventSequence,
        Account account,
        IEnumerable<FundAmount> startingFundBalances)
        : base(accountingPeriodId, eventDate, eventSequence, account) =>
        FundAmounts = startingFundBalances.ToList();

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private AccountAddedBalanceEvent() => FundAmounts = null!;
}