using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.Accounts;

/// <summary>
/// Entity class representing an Account
/// </summary> 
/// <remarks>
/// An Account represents a financial account that money can be held in and transferred from.
/// </remarks>
public class Account : Entity<AccountId>
{
    private readonly List<AccountBalanceCheckpoint> _accountBalanceCheckpoints = [];

    /// <summary>
    /// Name for this Account
    /// </summary>
    public string Name { get; internal set; }

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
    public AccountAddedBalanceEvent AccountAddedBalanceEvent { get; internal set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="name">Name for this Account</param>
    /// <param name="type">Type for this Account</param>
    internal Account(string name, AccountType type)
        : base(new AccountId(Guid.NewGuid()))
    {
        Name = name;
        Type = type;
        AccountAddedBalanceEvent = null!;
    }

    /// <summary>
    /// Adds a new Account Balance Checkpoint to this Account
    /// </summary>
    /// <param name="accountingPeriodId">Accounting Period ID for this Account Balance Checkpoint</param>
    /// <param name="fundBalances">Fund Balances for this Account Balance Checkpoint</param>
    internal void AddAccountBalanceCheckpoint(AccountingPeriodId accountingPeriodId, IEnumerable<FundAmount> fundBalances) =>
        _accountBalanceCheckpoints.Add(new AccountBalanceCheckpoint(this, accountingPeriodId, fundBalances));

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Account() : base()
    {
        Name = "";
        AccountAddedBalanceEvent = null!;
    }
}

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
}