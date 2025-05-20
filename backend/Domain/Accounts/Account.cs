using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.Accounts;

/// <summary>
/// Entity class representing an Account
/// </summary> 
/// <remarks>
/// An Account represents a financial account that money can be held in and transferred from.
/// </remarks>
public class Account
{
    private readonly List<AccountBalanceCheckpoint> _accountBalanceCheckpoints = [];

    /// <summary>
    /// ID for this Account
    /// </summary>
    public AccountId Id { get; private set; }

    /// <summary>
    /// Name for this Account
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Type for this Account
    /// </summary>
    public AccountType Type { get; private set; }

    /// <summary>
    /// Account Balance Checkpoints for this Account
    /// </summary>
    public IReadOnlyCollection<AccountBalanceCheckpoint> AccountBalanceCheckpoints => _accountBalanceCheckpoints;

    /// <summary>
    /// Account Added Balance Event for this Account
    /// </summary>
    public AccountAddedBalanceEvent AccountAddedBalanceEvent { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="name">Name for this Account</param>
    /// <param name="type">Type for this Account</param>
    /// <param name="accountingPeriod">First Accounting Period for this Account</param>
    /// <param name="date">First Date for this Account</param>
    /// <param name="startingFundBalances">Starting Fund Balances for this Account</param>
    internal Account(
        string name,
        AccountType type,
        AccountingPeriod accountingPeriod,
        DateOnly date,
        IEnumerable<FundAmount> startingFundBalances)
    {
        Id = new AccountId(Guid.NewGuid());
        Name = name;
        Type = type;
        AccountAddedBalanceEvent = new AccountAddedBalanceEvent(accountingPeriod, this, date, startingFundBalances);
    }

    /// <summary>
    /// Adds a new Account Balance Checkpoint to this Account
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period for this Account Balance Checkpoint</param>
    /// <param name="fundBalances">Fund Balances for this Account Balance Checkpoint</param>
    internal void AddAccountBalanceCheckpoint(AccountingPeriod accountingPeriod, IEnumerable<FundAmount> fundBalances) =>
        _accountBalanceCheckpoints.Add(new AccountBalanceCheckpoint(this, accountingPeriod, fundBalances));

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Account()
    {
        Id = null!;
        Name = "";
        AccountAddedBalanceEvent = null!;
    }
}

/// <summary>
/// Value object class representing the ID of an Account
/// </summary>
/// <param name="Value">Value for this Account ID</param>
public record AccountId(Guid Value) : EntityId(Value);

/// <summary>
/// Enum representing the different Account types
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Standard Account
    /// </summary>
    /// <remarks>
    /// A Standard Account represents a standard checking or savings account.
    /// Debiting a Standard Account will decrease its balance and crediting a
    /// Standard Account will increase its balance.
    /// </remarks>
    Standard,

    /// <summary>
    /// Debt Account
    /// </summary>
    /// <remarks>
    /// A Debt Account represents a credit card or loan account.
    /// Debiting a Debt Account will increase its balance and crediting a 
    /// Debt Account will decrease its balance.
    /// </remarks>
    Debt,

    /// <summary>
    /// Investment Account
    /// </summary>
    /// <remarks>
    /// An Investment Account represents a retirement or brokerage account.
    /// Investment Accounts are similar to Standard Accounts, however they
    /// also experience periodic changes in value as the value of the assets
    /// in the Account change.
    /// </remarks>
    Investment,
}