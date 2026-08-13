using Domain.Transactions.Income;

namespace Domain.AccountingPeriods;

/// <summary>
/// Expected income from a named source during an Accounting Period.
/// </summary>
public sealed class ExpectedIncomeSource : Entity<ExpectedIncomeSourceId>
{
    private List<IncomeLine> _incomeLines = [];
    private List<IncomeDeduction> _incomeDeductions = [];
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
    /// Income lines expected for each payment.
    /// </summary>
    public IReadOnlyCollection<IncomeLine> IncomeLines
    {
        get => _incomeLines;
        private set => _incomeLines = value.ToList();
    }

    /// <summary>
    /// Deductions expected for each payment.
    /// </summary>
    public IReadOnlyCollection<IncomeDeduction> IncomeDeductions
    {
        get => _incomeDeductions;
        private set => _incomeDeductions = value.ToList();
    }

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
    public decimal NetAmount => IncomeLines.Sum(line => line.Amount) - IncomeDeductions.Sum(deduction => deduction.Amount);

    /// <summary>
    /// Total amount expected from this source during the Accounting Period.
    /// </summary>
    public decimal ExpectedAmount => NetAmount * ExpectedDates.Count;

    /// <summary>
    /// Constructs an expected income source.
    /// </summary>
    internal ExpectedIncomeSource(
        AccountingPeriod accountingPeriod,
        string name,
        IEnumerable<IncomeLine> incomeLines,
        IEnumerable<IncomeDeduction> incomeDeductions,
        IEnumerable<DateOnly> expectedDates)
        : base(new ExpectedIncomeSourceId(Guid.NewGuid()))
    {
        AccountingPeriod = accountingPeriod;
        Name = name;
        _incomeLines.AddRange(incomeLines);
        _incomeDeductions.AddRange(incomeDeductions);
        _expectedDates.AddRange(expectedDates.Select(date => new ExpectedIncomeDate(date)));
    }

    /// <summary>
    /// Constructs a default instance for Entity Framework.
    /// </summary>
    private ExpectedIncomeSource() : base()
    {
        Name = null!;
        AccountingPeriod = null!;
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