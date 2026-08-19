using Domain.Funds;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity class representing the balances of a Fund within an Accounting Period.
/// </summary>
public class AccountingPeriodFundBalanceHistory : Entity<AccountingPeriodFundBalanceHistoryId>
{
    /// <summary>
    /// Fund for this Accounting Period Fund Balance History
    /// </summary>
    public Fund Fund { get; init; }

    /// <summary>
    /// Accounting Period for this Accounting Period Fund Balance History
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; init; }

    /// <summary>
    /// Opening Balance for this Accounting Period Fund Balance History
    /// </summary>
    public decimal OpeningBalance { get; private set; }

    /// <summary>
    /// Closing Balance for this Fund Accounting Period Balance History
    /// </summary>
    public decimal ClosingBalance { get; private set; }

    /// <summary>
    /// Gets the opening Fund Balance for this Fund Accounting Period Balance History
    /// </summary>
    public FundBalance GetOpeningFundBalance() => new(Fund, OpeningBalance);

    /// <summary>
    /// Gets the closing Fund Balance for this Fund Accounting Period Balance History
    /// </summary>
    public FundBalance GetClosingFundBalance() => new(Fund, ClosingBalance);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountingPeriodFundBalanceHistory(
        Fund fund,
        AccountingPeriod accountingPeriod,
        FundBalance openingBalance,
        FundBalance closingBalance)

        : base(new AccountingPeriodFundBalanceHistoryId(Guid.NewGuid()))
    {
        Fund = fund;
        AccountingPeriod = accountingPeriod;
        OpeningBalance = openingBalance.PostedBalance;
        ClosingBalance = closingBalance.PostedBalance;
    }

    /// <summary>
    /// Updates this Fund Accounting Period Balance History
    /// </summary>
    internal void Update(FundBalance openingBalance, FundBalance closingBalance)
    {
        OpeningBalance = openingBalance.PostedBalance;
        ClosingBalance = closingBalance.PostedBalance;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private AccountingPeriodFundBalanceHistory()
    {
        AccountingPeriod = null!;
        Fund = null!;
    }
}

/// <summary>
/// Value object class representing the ID of a <see cref="AccountingPeriodFundBalanceHistory"/>
/// </summary>
public record AccountingPeriodFundBalanceHistoryId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal AccountingPeriodFundBalanceHistoryId(Guid value)
        : base(value)
    {
    }
}
