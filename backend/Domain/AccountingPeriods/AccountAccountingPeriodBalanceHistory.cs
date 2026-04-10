using Domain.Accounts;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity class representing the balances of an Account within an Accounting Period.
/// </summary>
public class AccountAccountingPeriodBalanceHistory : Entity<AccountAccountingPeriodBalanceHistoryId>
{
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
    public decimal OpeningBalance { get; internal set; }

    /// <summary>
    /// Closing Balance for this Account Accounting Period Balance History
    /// </summary>
    public decimal ClosingBalance { get; internal set; }

    /// <summary>
    /// Gets the opening Account Balance for this Account Accounting Period Balance History
    /// </summary>
    public AccountBalance GetOpeningAccountBalance() => new(Account, OpeningBalance, 0, 0);

    /// <summary>
    /// Gets the closing Account Balance for this Account Accounting Period Balance History
    /// </summary>
    public AccountBalance GetClosingAccountBalance() => new(Account, ClosingBalance, 0, 0);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountAccountingPeriodBalanceHistory(
        Account account,
        AccountingPeriod accountingPeriod,
        decimal openingBalance,
        decimal closingBalance)
        : base(new AccountAccountingPeriodBalanceHistoryId(Guid.NewGuid()))
    {
        Account = account;
        AccountingPeriod = accountingPeriod;
        OpeningBalance = openingBalance;
        ClosingBalance = closingBalance;
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