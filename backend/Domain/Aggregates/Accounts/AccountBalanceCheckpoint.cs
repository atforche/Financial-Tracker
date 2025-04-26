using Domain.Aggregates.AccountingPeriods;
using Domain.ValueObjects;

namespace Domain.Aggregates.Accounts;

/// <summary>
/// Entity class representing an Account Balance Checkpoint
/// </summary>
/// <remarks>
/// An Account Balance Checkpoint represents the balance of an Account at the beginning of an Accounting Period.
/// This saved balance serves as a checkpoint to improve the efficiency of calculating arbitrary Account balances.
/// </remarks>
public sealed class AccountBalanceCheckpoint : EntityBase
{
    private readonly List<FundAmount> _fundBalances;

    /// <summary>
    /// Account for this Account Balance Checkpoint
    /// </summary>
    public Account Account { get; private set; }

    /// <summary>
    /// Accounting Period Year for this Account Balance Checkpoint
    /// </summary>
    public int AccountingPeriodYear { get; private set; }

    /// <summary>
    /// Accounting Period Month for this Account Balance Checkpoint
    /// </summary>
    public int AccountingPeriodMonth { get; private set; }

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
    /// <param name="accountingPeriod">Parent Accounting Period for this Account Balance Checkpoint</param>
    /// <param name="fundBalances">Collection of Fund Balances for this Account Balance Checkpoint</param>
    internal AccountBalanceCheckpoint(Account account, AccountingPeriod accountingPeriod, IEnumerable<FundAmount> fundBalances)
        : base(new EntityId(default, Guid.NewGuid()))
    {
        Account = account;
        AccountingPeriodYear = accountingPeriod.Year;
        AccountingPeriodMonth = accountingPeriod.Month;
        _fundBalances = fundBalances.ToList();
        Validate();
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private AccountBalanceCheckpoint()
        : base(new EntityId(default, Guid.NewGuid()))
    {
        Account = null!;
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