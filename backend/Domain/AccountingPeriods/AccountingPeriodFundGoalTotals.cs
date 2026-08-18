using Domain.FundGoals;
using Domain.Funds;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity representing Fund Goal totals within an Accounting Period.
/// </summary>
public sealed class AccountingPeriodFundGoalTotals : Entity<AccountingPeriodFundGoalTotalsId>
{
    /// <summary>
    /// Fund associated with this totals.
    /// </summary>
    public Fund Fund { get; init; }

    /// <summary>
    /// Accounting Period associated with this totals.
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; init; }

    /// <summary>
    /// Posted amount assigned during the Accounting Period.
    /// </summary>
    public decimal AmountAssigned { get; private set; }

    /// <summary>
    /// Posted amount assigned toward the regular monthly contribution during
    /// the Accounting Period.
    /// </summary>
    public decimal RegularAmountAssigned { get; private set; }

    /// <summary>
    /// Posted amount spent during the Accounting Period.
    /// </summary>
    public decimal AmountSpent { get; private set; }

    /// <summary>
    /// Gets the current Fund Goal totals.
    /// </summary>
    public FundGoalTotals GetTotals() =>
        new(Fund.Id, AmountAssigned, AmountSpent, RegularAmountAssigned);

    /// <summary>
    /// Updates this Accounting Period totals.
    /// </summary>
    internal void Update(FundGoalTotals totals)
    {
        AmountAssigned = totals.AmountAssigned;
        RegularAmountAssigned = totals.RegularAmountAssigned;
        AmountSpent = totals.AmountSpent;
    }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal AccountingPeriodFundGoalTotals(
        Fund fund,
        AccountingPeriod accountingPeriod,
        FundGoalTotals totals)
        : base(new AccountingPeriodFundGoalTotalsId(Guid.NewGuid()))
    {
        Fund = fund;
        AccountingPeriod = accountingPeriod;
        Update(totals);
    }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    private AccountingPeriodFundGoalTotals()
    {
        Fund = null!;
        AccountingPeriod = null!;
    }
}

/// <summary>
/// Value object representing the ID of an <see cref="AccountingPeriodFundGoalTotals"/>.
/// </summary>
public sealed record AccountingPeriodFundGoalTotalsId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal AccountingPeriodFundGoalTotalsId(Guid value) : base(value) { }
}
