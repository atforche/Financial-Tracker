using System.Diagnostics.CodeAnalysis;
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
public class AccountAddedBalanceEvent : Entity<AccountAddedBalanceEventId>, IBalanceEvent
{
    /// <summary>
    /// Account for this Account Added Balance Event
    /// </summary>
    public Account Account { get; private set; }

    /// <inheritdoc/>
    public AccountingPeriodId AccountingPeriodId { get; private set; }

    /// <inheritdoc/>
    public DateOnly EventDate { get; private set; }

    /// <inheritdoc/>
    public int EventSequence { get; private set; }

    /// <summary>
    /// Fund Amounts for this Account Added Balance Event
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundAmounts { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyCollection<AccountId> GetAccountIds() => [Account.Id];

    /// <inheritdoc/>
    public bool IsValidToApplyToBalance(AccountBalance currentBalance, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;
        return true;
    }

    /// <inheritdoc/>
    public AccountBalance ApplyEventToBalance(AccountBalance currentBalance, ApplicationDirection direction) =>
        direction == ApplicationDirection.Standard
            ? new(Account, FundAmounts, [])
            : new(Account, [], []);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriodId">Accounting Period ID for this Account Added Balance Event</param>
    /// <param name="eventDate">Event Date for this Account Added Balance Event</param>
    /// <param name="eventSequence">Event Sequence for this Account Added Balance Event</param>
    /// <param name="account">Account for this Account Added Balance Event</param>
    /// <param name="startingFundBalances">Starting Fund Balances for this Account Added Balance Event</param>
    internal AccountAddedBalanceEvent(
        Account account,
        AccountingPeriodId accountingPeriodId,
        DateOnly eventDate,
        int eventSequence,
        IEnumerable<FundAmount> startingFundBalances)
        : base(new AccountAddedBalanceEventId(Guid.NewGuid()))
    {
        Account = account;
        AccountingPeriodId = accountingPeriodId;
        EventDate = eventDate;
        EventSequence = eventSequence;
        FundAmounts = startingFundBalances.ToList();
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private AccountAddedBalanceEvent()
        : base()
    {
        Account = null!;
        AccountingPeriodId = null!;
        FundAmounts = null!;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="AccountAddedBalanceEvent"/>
/// </summary>
public record AccountAddedBalanceEventId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// This constructor should only even be used when creating a new Account Added Balance Event ID during 
    /// Account creation. 
    /// </summary>
    /// <param name="value">Value for this Account Added Balance Event ID</param>
    internal AccountAddedBalanceEventId(Guid value)
        : base(value)
    {
    }
}