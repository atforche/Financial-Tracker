using Domain.Accounts;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity class representing the balances of an Account within an Accounting Period.
/// </summary>
public class AccountingPeriodAccountBalanceHistory : Entity<AccountingPeriodAccountBalanceHistoryId>
{
    /// <summary>
    /// Account for this Accounting Period Account Balance History
    /// </summary>
    public Account Account { get; init; }

    /// <summary>
    /// Accounting Period for this Accounting Period Account Balance History
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; init; }

    /// <summary>
    /// Opening Balance for this Accounting Period Account Balance History
    /// </summary>
    public decimal OpeningBalance { get; internal set; }

    /// <summary>
    /// Closing Balance for this Accounting Period Account Balance History
    /// </summary>
    public decimal ClosingBalance { get; internal set; }

    /// <summary>
    /// Gets the opening Account Balance for this Accounting Period Account Balance History
    /// </summary>
    public AccountBalance GetOpeningAccountBalance() => new(Account, OpeningBalance, 0, 0);

    /// <summary>
    /// Gets the closing Account Balance for this Accounting Period Account Balance History
    /// </summary>
    public AccountBalance GetClosingAccountBalance() => new(Account, ClosingBalance, 0, 0);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountingPeriodAccountBalanceHistory(
        Account account,
        AccountingPeriod accountingPeriod,
        decimal openingBalance,
        decimal closingBalance)
        : base(new AccountingPeriodAccountBalanceHistoryId(Guid.NewGuid()))
    {
        Account = account;
        AccountingPeriod = accountingPeriod;
        OpeningBalance = openingBalance;
        ClosingBalance = closingBalance;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private AccountingPeriodAccountBalanceHistory()
    {
        AccountingPeriod = null!;
        Account = null!;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="AccountingPeriodAccountBalanceHistory"/>
/// </summary>
public record AccountingPeriodAccountBalanceHistoryId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountingPeriodAccountBalanceHistoryId(Guid value)
        : base(value)
    {
    }
}