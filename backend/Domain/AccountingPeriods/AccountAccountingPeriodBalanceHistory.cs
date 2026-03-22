using Domain.Accounts;
using Domain.Funds;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity class representing the balances of an Account within an Accounting Period.
/// </summary>
public class AccountAccountingPeriodBalanceHistory : Entity<AccountAccountingPeriodBalanceHistoryId>
{
    private List<FundAmount> _openingFundBalances = [];
    private List<FundAmount> _closingFundBalances = [];

    /// <summary>
    /// Account for this Account Accounting Period Balance History
    /// </summary>
    public Account Account { get; init; }

    /// <summary>
    /// Accounting Period for this Account Accounting Period Balance History
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; init; }

    /// <summary>
    /// Opening Balance for this Account Accounting Period Balance History
    /// </summary>
    public decimal OpeningBalance { get => OpeningFundBalances.Sum(f => f.Amount); internal set { } }

    /// <summary>
    /// Opening Fund Balances for this Account Accounting Period Balance History
    /// </summary>
    public IReadOnlyCollection<FundAmount> OpeningFundBalances
    {
        get => _openingFundBalances;
        internal set => _openingFundBalances = value.ToList();
    }

    /// <summary>
    /// Gets the opening Account Balance for this Account Accounting Period Balance History
    /// </summary>
    public AccountBalance GetOpeningAccountBalance() => new(Account, OpeningFundBalances, [], []);

    /// <summary>
    /// Closing Balance for this Account Accounting Period Balance History
    /// </summary>
    public decimal ClosingBalance { get => ClosingFundBalances.Sum(f => f.Amount); internal set { } }

    /// <summary>
    /// Closing Fund Balances for this Account Accounting Period Balance History
    /// </summary>
    public IReadOnlyCollection<FundAmount> ClosingFundBalances
    {
        get => _closingFundBalances;
        internal set => _closingFundBalances = value.ToList();
    }

    /// <summary>
    /// Gets the closing Account Balance for this Account Accounting Period Balance History
    /// </summary>
    public AccountBalance GetClosingAccountBalance() => new(Account, ClosingFundBalances, [], []);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountAccountingPeriodBalanceHistory(
        Account account,
        AccountingPeriod accountingPeriod,
        IEnumerable<FundAmount> openingFundBalances,
        IEnumerable<FundAmount> closingFundBalances)
        : base(new AccountAccountingPeriodBalanceHistoryId(Guid.NewGuid()))
    {
        Account = account;
        AccountingPeriod = accountingPeriod;
        _openingFundBalances = openingFundBalances.ToList();
        _closingFundBalances = closingFundBalances.ToList();
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private AccountAccountingPeriodBalanceHistory()
    {
        AccountingPeriod = null!;
        Account = null!;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="AccountAccountingPeriodBalanceHistory"/>
/// </summary>
public record AccountAccountingPeriodBalanceHistoryId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountAccountingPeriodBalanceHistoryId(Guid value)
        : base(value)
    {
    }
}