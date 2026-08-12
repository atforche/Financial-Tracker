using Domain.Income;
using Domain.Payroll;

namespace Domain.AccountingPeriods;

/// <summary>
/// Expected income from a named source during an Accounting Period.
/// </summary>
public sealed class ExpectedIncomeSource : Entity<ExpectedIncomeSourceId>
{
    private List<ExpectedIncomeDate> _expectedDates = [];

    /// <summary>
    /// Name used to identify this expected income source.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Accounting Period that owns this source.
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; private set; }

    /// <summary>
    /// Economic composition expected for each payment.
    /// </summary>
    public IncomeBreakdown Income { get; private set; }

    /// <summary>
    /// Number of payments made by this source during a full year.
    /// </summary>
    public int? PayPeriodsPerYear => (Income as ExpectedPayrollPayment)?.PayPeriodsPerYear;

    /// <summary>
    /// Withholding inputs used to project this expected payroll income.
    /// </summary>
    public PayrollWithholdingConfiguration? WithholdingConfiguration =>
        (Income as ExpectedPayrollPayment)?.WithholdingConfiguration;

    /// <summary>
    /// Dates on which this source is expected to pay during the Accounting Period.
    /// </summary>
    public IReadOnlyCollection<ExpectedIncomeDate> ExpectedDates
    {
        get => _expectedDates;
        private set => _expectedDates = value.ToList();
    }

    /// <summary>
    /// Net amount expected for one payment.
    /// </summary>
    public decimal TrackedAmount => Income.TrackedAmount;

    /// <summary>
    /// Untracked income expected for one payment.
    /// </summary>
    public decimal UntrackedAmount => Income.UntrackedAmount;

    /// <summary>
    /// Total recognized income expected for one payment.
    /// </summary>
    public decimal Amount => Income.TotalAmount;

    /// <summary>
    /// Total amount expected from this source during the Accounting Period.
    /// </summary>
    public decimal ExpectedTrackedAmount => TrackedAmount * ExpectedDates.Count;

    /// <summary>
    /// Total untracked income expected during the Accounting Period.
    /// </summary>
    public decimal ExpectedUntrackedAmount => UntrackedAmount * ExpectedDates.Count;

    /// <summary>
    /// Total recognized income expected during the Accounting Period.
    /// </summary>
    public decimal ExpectedAmount => Amount * ExpectedDates.Count;

    /// <summary>
    /// Constructs an expected income source.
    /// </summary>
    internal ExpectedIncomeSource(
        AccountingPeriod accountingPeriod,
        string name,
        IncomeBreakdown income,
        IEnumerable<DateOnly> expectedDates)
        : base(new ExpectedIncomeSourceId(Guid.NewGuid()))
    {
        AccountingPeriod = accountingPeriod;
        Name = name;
        Income = income;
        _expectedDates.AddRange(expectedDates.Select(date => new ExpectedIncomeDate(date)));
    }

    /// <summary>
    /// Constructs a default instance for Entity Framework.
    /// </summary>
    private ExpectedIncomeSource() : base()
    {
        Name = null!;
        AccountingPeriod = null!;
        Income = null!;
    }
}

/// <summary>
/// A calendar date on which an expected income source should pay.
/// </summary>
public sealed class ExpectedIncomeDate(DateOnly date)
{
    /// <summary>
    /// Expected payment date.
    /// </summary>
    public DateOnly Date { get; private set; } = date;

    /// <summary>
    /// Constructs a default instance for Entity Framework.
    /// </summary>
    private ExpectedIncomeDate() : this(default) { }
}

/// <summary>
/// Identifier for an <see cref="ExpectedIncomeSource"/>.
/// </summary>
public sealed record ExpectedIncomeSourceId : EntityId
{
    internal ExpectedIncomeSourceId(Guid value) : base(value) { }
}