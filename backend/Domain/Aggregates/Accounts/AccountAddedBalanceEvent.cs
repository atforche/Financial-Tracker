using Domain.Aggregates.AccountingPeriods;
using Domain.ValueObjects;

namespace Domain.Aggregates.Accounts;

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

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for this Balance Event</param>
    /// <param name="account">Account for this Balance Event</param>
    /// <param name="eventDate">Event Date for this Balance Event</param>
    /// <param name="startingFundBalances">Starting Fund Balances for this Account Added Balance Event</param>
    internal AccountAddedBalanceEvent(
        AccountingPeriod accountingPeriod,
        Account account,
        DateOnly eventDate,
        IEnumerable<FundAmount> startingFundBalances)
        : base(accountingPeriod, account, eventDate) =>
        FundAmounts = startingFundBalances.ToList();

    /// <inheritdoc/>
    public override AccountBalance ApplyEventToBalance(AccountBalance currentBalance) => new(Account, FundAmounts, []);

    /// <inheritdoc/>
    public override AccountBalance ReverseEventFromBalance(AccountBalance currentBalance) => new(Account, [], []);
}