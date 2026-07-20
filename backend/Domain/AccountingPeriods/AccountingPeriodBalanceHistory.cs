using Domain.Accounts;
using Domain.Funds;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity class representing the balances of an Accounting Period.
/// </summary>
public class AccountingPeriodBalanceHistory : Entity<AccountingPeriodBalanceHistoryId>
{
    private List<AccountingPeriodAccountBalanceHistory> _accountBalances = [];
    private List<AccountingPeriodFundBalanceHistory> _fundBalances = [];
    private List<AccountingPeriodFundPlanTotals> _fundPlanTotals = [];
    private List<AccountingPeriodFundPlanSnapshot> _fundPlanSnapshots = [];

    /// <summary>
    /// Accounting Period for this Accounting Period Balance History
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; init; }

    /// <summary>
    /// Opening Balance for this Accounting Period Balance History
    /// </summary>
    public decimal OpeningBalance { get; private set; }

    /// <summary>
    /// Closing Balance for this Accounting Period Balance History
    /// </summary>
    public decimal ClosingBalance { get; private set; }

    /// <summary>
    /// Account Balances for this Accounting Period Balance History
    /// </summary>
    public IReadOnlyCollection<AccountingPeriodAccountBalanceHistory> AccountBalances
    {
        get => _accountBalances;
        private set => _accountBalances = value.ToList();
    }

    /// <summary>
    /// Fund Balances for this Accounting Period Balance History
    /// </summary>
    public IReadOnlyCollection<AccountingPeriodFundBalanceHistory> FundBalances
    {
        get => _fundBalances;
        private set => _fundBalances = value.ToList();
    }

    /// <summary>
    /// Fund Plan totals for this Accounting Period Balance History.
    /// </summary>
    public IReadOnlyCollection<AccountingPeriodFundPlanTotals> FundPlanTotals
    {
        get => _fundPlanTotals;
        private set => _fundPlanTotals = value.ToList();
    }

    /// <summary>
    /// Fund Plan configuration snapshots for this Accounting Period Balance History.
    /// </summary>
    public IReadOnlyCollection<AccountingPeriodFundPlanSnapshot> FundPlanSnapshots
    {
        get => _fundPlanSnapshots;
        private set => _fundPlanSnapshots = value.ToList();
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountingPeriodBalanceHistory(
        AccountingPeriod accountingPeriod,
        IEnumerable<AccountingPeriodAccountBalanceHistory> accountBalances,
        IEnumerable<AccountingPeriodFundBalanceHistory> fundBalances,
        IEnumerable<AccountingPeriodFundPlanTotals> fundPlanTotals,
        IEnumerable<AccountingPeriodFundPlanSnapshot> fundPlanSnapshots)
        : base(new AccountingPeriodBalanceHistoryId(Guid.NewGuid()))
    {
        AccountingPeriod = accountingPeriod;
        _accountBalances = accountBalances.ToList();
        _fundBalances = fundBalances.ToList();
        _fundPlanTotals = fundPlanTotals.ToList();
        _fundPlanSnapshots = fundPlanSnapshots.ToList();
        UpdateBalances();
    }

    /// <summary>
    /// Updates the opening and closing balances for this Accounting Period Balance History
    /// </summary>
    internal void UpdateBalances()
    {
        OpeningBalance = AccountBalances.Sum(accountBalance => accountBalance.Account.Type.IsDebt() ? -accountBalance.OpeningBalance : accountBalance.OpeningBalance);
        ClosingBalance = AccountBalances.Sum(accountBalance => accountBalance.Account.Type.IsDebt() ? -accountBalance.ClosingBalance : accountBalance.ClosingBalance);
    }

    /// <summary>
    /// Adds an Account Balance to this Accounting Period Balance History.
    /// </summary>
    internal void AddAccountBalance(AccountingPeriodAccountBalanceHistory accountBalance)
    {
        _accountBalances.Add(accountBalance);
        UpdateBalances();
    }

    /// <summary>
    /// Removes the Account Balance for the provided Account from this Accounting Period Balance History.
    /// </summary>
    internal void RemoveAccountBalance(AccountId accountId)
    {
        _ = _accountBalances.RemoveAll(accountBalance => accountBalance.Account.Id == accountId);
        UpdateBalances();
    }

    /// <summary>
    /// Adds a Fund Balance to this Accounting Period Balance History.
    /// </summary>
    internal void AddFundBalance(AccountingPeriodFundBalanceHistory fundBalance) => _fundBalances.Add(fundBalance);

    /// <summary>
    /// Removes the Fund Balance for the provided Fund from this Accounting Period Balance History.
    /// </summary>
    internal void RemoveFundBalance(FundId fundId) =>
        _ = _fundBalances.RemoveAll(fundBalance => fundBalance.Fund.Id == fundId);

    /// <summary>
    /// Adds Fund Plan totals to this Accounting Period Balance History.
    /// </summary>
    internal void AddFundPlanTotals(AccountingPeriodFundPlanTotals totals) => _fundPlanTotals.Add(totals);

    /// <summary>
    /// Adds a Fund Plan configuration snapshot to this Accounting Period Balance History.
    /// </summary>
    internal void AddFundPlanSnapshot(AccountingPeriodFundPlanSnapshot snapshot) => _fundPlanSnapshots.Add(snapshot);

    /// <summary>
    /// Removes Fund Plan totals for the provided Fund from this Accounting Period Balance History.
    /// </summary>
    internal void RemoveFundPlanTotals(FundId fundId) =>
        _ = _fundPlanTotals.RemoveAll(totals => totals.Fund.Id == fundId);

    /// <summary>
    /// Removes the Fund Plan configuration snapshot for the provided Fund.
    /// </summary>
    internal void RemoveFundPlanSnapshot(FundId fundId) =>
        _ = _fundPlanSnapshots.RemoveAll(snapshot => snapshot.Fund.Id == fundId);

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