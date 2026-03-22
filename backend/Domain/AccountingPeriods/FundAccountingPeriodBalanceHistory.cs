using Domain.Accounts;
using Domain.Funds;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity class representing the balances of a Fund within an Accounting Period.
/// </summary>
public class FundAccountingPeriodBalanceHistory : Entity<FundAccountingPeriodBalanceHistoryId>
{
    private List<AccountAmount> _openingAccountBalances = [];
    private List<AccountAmount> _closingAccountBalances = [];

    /// <summary>
    /// Fund for this Fund Accounting Period Balance History
    /// </summary>
    public Fund Fund { get; init; }

    /// <summary>
    /// Accounting Period for this Fund Accounting Period Balance History
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; init; }

    /// <summary>
    /// Opening Balance for this Fund Accounting Period Balance History
    /// </summary>
    public decimal OpeningBalance { get => OpeningAccountBalances.Sum(a => a.Amount); internal set { } }

    /// <summary>
    /// Opening Account Balances for this Fund Accounting Period Balance History
    /// </summary>
    public IReadOnlyCollection<AccountAmount> OpeningAccountBalances
    {
        get => _openingAccountBalances;
        internal set => _openingAccountBalances = value.ToList();
    }

    /// <summary>
    /// Gets the opening Fund Balance for this Fund Accounting Period Balance History
    /// </summary>
    public FundBalance GetOpeningFundBalance() => new(Fund.Id, OpeningAccountBalances, [], []);

    /// <summary>
    /// Closing Balance for this Fund Accounting Period Balance History
    /// </summary>
    public decimal ClosingBalance { get => ClosingAccountBalances.Sum(a => a.Amount); internal set { } }

    /// <summary>
    /// Closing Account Balances for this Fund Accounting Period Balance History
    /// </summary>
    public IReadOnlyCollection<AccountAmount> ClosingAccountBalances
    {
        get => _closingAccountBalances;
        internal set => _closingAccountBalances = value.ToList();
    }

    /// <summary>
    /// Gets the closing Fund Balance for this Fund Accounting Period Balance History
    /// </summary>
    public FundBalance GetClosingFundBalance() => new(Fund.Id, ClosingAccountBalances, [], []);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundAccountingPeriodBalanceHistory(
        Fund fund,
        AccountingPeriod accountingPeriod,
        IEnumerable<AccountAmount> openingAccountBalances,
        IEnumerable<AccountAmount> closingAccountBalances)
        : base(new FundAccountingPeriodBalanceHistoryId(Guid.NewGuid()))
    {
        Fund = fund;
        AccountingPeriod = accountingPeriod;
        _openingAccountBalances = openingAccountBalances.ToList();
        _closingAccountBalances = closingAccountBalances.ToList();
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private FundAccountingPeriodBalanceHistory()
    {
        AccountingPeriod = null!;
        Fund = null!;
    }
}

/// <summary>
/// Value object class representing the ID of a <see cref="FundAccountingPeriodBalanceHistory"/>
/// </summary>
public record FundAccountingPeriodBalanceHistoryId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundAccountingPeriodBalanceHistoryId(Guid value)
        : base(value)
    {
    }
}