using Domain.Funds;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity class representing the balances of a Fund within an Accounting Period.
/// </summary>
public class FundAccountingPeriodBalanceHistory : Entity<FundAccountingPeriodBalanceHistoryId>
{
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
    public decimal OpeningBalance { get; internal set; }

    /// <summary>
    /// Closing Balance for this Fund Accounting Period Balance History
    /// </summary>
    public decimal ClosingBalance { get; internal set; }

    /// <summary>
    /// Gets the opening Fund Balance for this Fund Accounting Period Balance History
    /// </summary>
    public FundBalance GetOpeningFundBalance() => new(Fund.Id, OpeningBalance, 0, 0);

    /// <summary>
    /// Gets the closing Fund Balance for this Fund Accounting Period Balance History
    /// </summary>
    public FundBalance GetClosingFundBalance() => new(Fund.Id, ClosingBalance, 0, 0);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundAccountingPeriodBalanceHistory(
        Fund fund,
        AccountingPeriod accountingPeriod,
        decimal openingBalance,
        decimal closingBalance)
        : base(new FundAccountingPeriodBalanceHistoryId(Guid.NewGuid()))
    {
        Fund = fund;
        AccountingPeriod = accountingPeriod;
        OpeningBalance = openingBalance;
        ClosingBalance = closingBalance;
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