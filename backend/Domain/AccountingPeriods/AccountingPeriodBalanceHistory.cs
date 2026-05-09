using Domain.Accounts;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity class representing the balances of an Accounting Period.
/// </summary>
public class AccountingPeriodBalanceHistory : Entity<AccountingPeriodBalanceHistoryId>
{
    private List<AccountingPeriodAccountBalanceHistory> _accountBalances = [];
    private List<AccountingPeriodFundBalanceHistory> _fundBalances = [];

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
        internal set => _accountBalances = value.ToList();
    }

    /// <summary>
    /// Fund Balances for this Accounting Period Balance History
    /// </summary>
    public IReadOnlyCollection<AccountingPeriodFundBalanceHistory> FundBalances
    {
        get => _fundBalances;
        internal set => _fundBalances = value.ToList();
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountingPeriodBalanceHistory(
        AccountingPeriod accountingPeriod,
        IEnumerable<AccountingPeriodAccountBalanceHistory> accountBalances,
        IEnumerable<AccountingPeriodFundBalanceHistory> fundBalances)
        : base(new AccountingPeriodBalanceHistoryId(Guid.NewGuid()))
    {
        AccountingPeriod = accountingPeriod;
        _accountBalances = accountBalances.ToList();
        _fundBalances = fundBalances.ToList();
    }

    /// <summary>
    /// Updates the opening and closing balances for this Accounting Period Balance History
    /// </summary>
    internal void UpdateBalances()
    {
        OpeningBalance = AccountBalances.Sum(accountBalance => accountBalance.Account.Type == AccountType.Debt ? -accountBalance.OpeningBalance : accountBalance.OpeningBalance);
        ClosingBalance = AccountBalances.Sum(accountBalance => accountBalance.Account.Type == AccountType.Debt ? -accountBalance.ClosingBalance : accountBalance.ClosingBalance);
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