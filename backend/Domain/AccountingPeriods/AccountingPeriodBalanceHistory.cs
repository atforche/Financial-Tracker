namespace Domain.AccountingPeriods;

/// <summary>
/// Entity class representing the balances of an Accounting Period.
/// </summary>
public class AccountingPeriodBalanceHistory : Entity<AccountingPeriodBalanceHistoryId>
{
    private List<AccountAccountingPeriodBalanceHistory> _accountBalances = [];
    private List<FundAccountingPeriodBalanceHistory> _fundBalances = [];

    /// <summary>
    /// Accounting Period for this Accounting Period Balance History
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; init; }

    /// <summary>
    /// Opening Balance for this Accounting Period Balance History
    /// </summary>
    public decimal OpeningBalance { get => FundBalances.Sum(f => f.OpeningBalance); internal set { } }

    /// <summary>
    /// Closing Balance for this Accounting Period Balance History
    /// </summary>
    public decimal ClosingBalance { get => FundBalances.Sum(f => f.ClosingBalance); internal set { } }

    /// <summary>
    /// Account Balances for this Accounting Period Balance History
    /// </summary>
    public IReadOnlyCollection<AccountAccountingPeriodBalanceHistory> AccountBalances
    {
        get => _accountBalances;
        internal set => _accountBalances = value.ToList();
    }

    /// <summary>
    /// Fund Balances for this Accounting Period Balance History
    /// </summary>
    public IReadOnlyCollection<FundAccountingPeriodBalanceHistory> FundBalances
    {
        get => _fundBalances;
        internal set => _fundBalances = value.ToList();
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountingPeriodBalanceHistory(
        AccountingPeriod accountingPeriod,
        IEnumerable<AccountAccountingPeriodBalanceHistory> accountBalances,
        IEnumerable<FundAccountingPeriodBalanceHistory> fundBalances)
        : base(new AccountingPeriodBalanceHistoryId(Guid.NewGuid()))
    {
        AccountingPeriod = accountingPeriod;
        _accountBalances = accountBalances.ToList();
        _fundBalances = fundBalances.ToList();
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private AccountingPeriodBalanceHistory()
    {
        AccountingPeriod = null!;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="AccountingPeriodBalanceHistory"/>
/// </summary>
public record AccountingPeriodBalanceHistoryId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountingPeriodBalanceHistoryId(Guid value)
        : base(value)
    {
    }
}