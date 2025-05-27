using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.Accounts;

/// <summary>
/// Entity class representing an Account Balance Checkpoint
/// </summary>
/// <remarks>
/// An Account Balance Checkpoint represents the balance of an Account at the beginning of an Accounting Period.
/// This saved balance serves as a checkpoint to improve the efficiency of calculating arbitrary Account balances.
/// </remarks>
public sealed class AccountBalanceCheckpoint : Entity<AccountBalanceCheckpointId>
{
    private readonly List<FundAmount> _fundBalances;

    /// <summary>
    /// Account for this Account Balance Checkpoint
    /// </summary>
    public Account Account { get; private set; }

    /// <summary>
    /// Accounting Period Id for this Account Balance Checkpoint
    /// </summary>
    public AccountingPeriodId AccountingPeriodId { get; private set; }

    /// <summary>
    /// Fund Balances for this Account Balance Checkpoint
    /// </summary>
    public IReadOnlyCollection<FundAmount> FundBalances => _fundBalances;

    /// <summary>
    /// Gets this Account Balance Checkpoint as an Account Balance
    /// </summary>
    /// <returns>This Account Balance Checkpoint as an Account Balance</returns>
    public AccountBalance ConvertToAccountBalance() => new(Account, FundBalances, []);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="account">Parent Account for this Account Balance Checkpoint</param>
    /// <param name="accountingPeriodId">Accounting Period ID for this Account Balance Checkpoint</param>
    /// <param name="fundBalances">Collection of Fund Balances for this Account Balance Checkpoint</param>
    internal AccountBalanceCheckpoint(Account account, AccountingPeriodId accountingPeriodId, IEnumerable<FundAmount> fundBalances)
        : base(new AccountBalanceCheckpointId(Guid.NewGuid()))
    {
        Account = account;
        AccountingPeriodId = accountingPeriodId;
        _fundBalances = fundBalances.ToList();
        Validate();
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private AccountBalanceCheckpoint() : base()
    {
        Account = null!;
        AccountingPeriodId = null!;
        _fundBalances = [];
    }

    /// <summary>
    /// Validates the current Account Balance Checkpoint
    /// </summary>
    private void Validate()
    {
        if (FundBalances.Sum(balance => balance.Amount) < 0)
        {
            throw new InvalidOperationException();
        }
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="AccountBalanceCheckpoint"/>
/// </summary>
public record AccountBalanceCheckpointId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// This constructor should only even be used when creating a new Account Balance Checkpoint ID during 
    /// Account Balance Checkpoint creation. 
    /// </summary>
    /// <param name="value">Value for this Account Balance Checkpoint ID</param>
    internal AccountBalanceCheckpointId(Guid value)
        : base(value)
    {
    }
}