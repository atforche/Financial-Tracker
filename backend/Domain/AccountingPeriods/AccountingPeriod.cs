using Domain.Accounts;
using Domain.BalanceEvents;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity class representing an Accounting Period
/// </summary>
/// <remarks>
/// An Accounting Period represents a month-long period used to organize transactions and track balances and budgets.
/// </remarks>
public class AccountingPeriod : Entity
{
    private readonly List<Transaction> _transactions = [];
    private readonly List<FundConversion> _fundConversions = [];
    private readonly List<ChangeInValue> _changeInValues = [];
    private readonly List<AccountAddedBalanceEvent> _accountAddedBalanceEvents = [];

    /// <summary>
    /// Key for this Accounting Period
    /// </summary>
    public AccountingPeriodKey Key { get; private set; }

    /// <summary>
    /// Year for this Accounting Period
    /// </summary>
    public int Year => Key.Year;

    /// <summary>
    /// Month for this Accounting Period
    /// </summary>
    public int Month => Key.Month;

    /// <summary>
    /// Gets the Period Start Date for this Accounting Period
    /// </summary>
    public DateOnly PeriodStartDate => Key.ConvertToDate();

    /// <summary>
    /// Is Open flag for this Accounting Period
    /// </summary>
    /// <remarks>
    /// Once an Accounting Period has been closed, no changes can be made to anything thats fall within 
    /// that Accounting Period. Multiple Accounting Periods can be open at the same time, assuming all 
    /// the open periods represent a contiguous period of time. Only the earliest open period can be closed.
    /// </remarks>
    public bool IsOpen { get; internal set; }

    /// <summary>
    /// Transactions for this Accounting Period
    /// </summary>
    public IReadOnlyCollection<Transaction> Transactions => _transactions;

    /// <summary>
    /// Fund Conversions for this Accounting Period
    /// </summary>
    public IReadOnlyCollection<FundConversion> FundConversions => _fundConversions;

    /// <summary>
    /// Change In Values for this Accounting Period
    /// </summary>
    public IReadOnlyCollection<ChangeInValue> ChangeInValues => _changeInValues;

    /// <summary>
    /// Account Added Balance Events for this Accounting Period
    /// </summary>
    public IReadOnlyCollection<AccountAddedBalanceEvent> AccountAddedBalanceEvents => _accountAddedBalanceEvents;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="year">Year for this Accounting Period</param>
    /// <param name="month">Month for this Accounting Period</param>
    internal AccountingPeriod(int year, int month)
        : base(new EntityId(default, Guid.NewGuid()))
    {
        Key = new AccountingPeriodKey(year, month);
        IsOpen = true;
    }

    /// <summary>
    /// Adds a new Transaction to this Accounting Period
    /// </summary>
    /// <param name="transaction">Transaction to add</param>
    internal void AddTransaction(Transaction transaction) => _transactions.Add(transaction);

    /// <summary>
    /// Adds a new Fund Conversion to this Accounting Period
    /// </summary>
    /// <param name="fundConversion">Fund Conversion to add</param>
    internal void AddFundConversion(FundConversion fundConversion) => _fundConversions.Add(fundConversion);

    /// <summary>
    /// Adds a new Change In Value to this Accounting Period
    /// </summary>
    /// <param name="changeInValue">Change in Value to add</param>
    internal void AddChangeInValue(ChangeInValue changeInValue) => _changeInValues.Add(changeInValue);

    /// <summary>
    /// Adds a new Account Added Balance Event to this Accounting Period
    /// </summary>
    /// <param name="accountAddedBalanceEvent">Account Added Balance Event to add</param>
    internal void AddAccountAddedBalanceEvent(AccountAddedBalanceEvent accountAddedBalanceEvent) =>
        _accountAddedBalanceEvents.Add(accountAddedBalanceEvent);

    /// <summary>
    /// Gets the list of all Balance Events for this Accounting Period
    /// </summary>
    /// <returns>The list of all Balance Events for this Accounting Period</returns>
    internal IEnumerable<BalanceEvent> GetAllBalanceEvents() =>
        Transactions.SelectMany(transaction => (IEnumerable<BalanceEvent>)transaction.TransactionBalanceEvents)
            .Concat(FundConversions)
            .Concat(ChangeInValues)
            .Concat(AccountAddedBalanceEvents)
            .OrderBy(balanceEvent => balanceEvent.EventDate)
            .ThenBy(balanceEvent => balanceEvent.EventSequence);

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private AccountingPeriod()
        : base(new EntityId(default, Guid.NewGuid())) =>
        Key = null!;
}