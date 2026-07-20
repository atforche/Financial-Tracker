using Domain.FundPlans;
using Domain.Funds;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity representing Fund Plan totals within an Accounting Period.
/// </summary>
public sealed class AccountingPeriodFundPlanTotals : Entity<AccountingPeriodFundPlanTotalsId>
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
    /// Pending amount assigned during the Accounting Period.
    /// </summary>
    public decimal PendingAmountAssigned { get; private set; }

    /// <summary>
    /// Posted amount spent during the Accounting Period.
    /// </summary>
    public decimal AmountSpent { get; private set; }

    /// <summary>
    /// Pending amount spent during the Accounting Period.
    /// </summary>
    public decimal PendingAmountSpent { get; private set; }

    /// <summary>
    /// Gets the current Fund Plan totals.
    /// </summary>
    public FundPlanTotals GetTotals() =>
        new(Fund.Id, AmountAssigned, PendingAmountAssigned, AmountSpent, PendingAmountSpent);

    /// <summary>
    /// Updates this Accounting Period totals.
    /// </summary>
    internal void Update(FundPlanTotals totals)
    {
        AmountAssigned = totals.AmountAssigned;
        PendingAmountAssigned = totals.PendingAmountAssigned;
        AmountSpent = totals.AmountSpent;
        PendingAmountSpent = totals.PendingAmountSpent;
    }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal AccountingPeriodFundPlanTotals(
        Fund fund,
        AccountingPeriod accountingPeriod,
        FundPlanTotals totals)
        : base(new AccountingPeriodFundPlanTotalsId(Guid.NewGuid()))
    {
        Fund = fund;
        AccountingPeriod = accountingPeriod;
        Update(totals);
    }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    private AccountingPeriodFundPlanTotals()
    {
        Fund = null!;
        AccountingPeriod = null!;
    }
}

/// <summary>
/// Value object representing the ID of an <see cref="AccountingPeriodFundPlanTotals"/>.
/// </summary>
public sealed record AccountingPeriodFundPlanTotalsId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal AccountingPeriodFundPlanTotalsId(Guid value) : base(value) { }
}