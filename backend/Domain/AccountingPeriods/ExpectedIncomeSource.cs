using Domain.Transactions.Income;

namespace Domain.AccountingPeriods;

/// <summary>
/// Expected income from a named source during an Accounting Period.
/// </summary>
public sealed class ExpectedIncomeSource : Entity<ExpectedIncomeSourceId>
{
    private List<IncomeLine> _incomeLines = [];
    private List<IncomeDeduction> _incomeDeductions = [];
    private List<ExpectedUntrackedIncomeTransfer> _untrackedTransfers = [];
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
    /// Transfers from each expected payment to untracked accounts.
    /// </summary>
    public IReadOnlyCollection<ExpectedUntrackedIncomeTransfer> UntrackedTransfers
    {
        get => _untrackedTransfers;
        private set => _untrackedTransfers = value.ToList();
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
    /// Amount expected to reach tracked accounts from one payment.
    /// </summary>
    public decimal TrackedAmount => NetAmount - UntrackedAmount;

    /// <summary>
    /// Amount expected to reach untracked accounts from one payment.
    /// </summary>
    public decimal UntrackedAmount => UntrackedTransfers.Sum(transfer => transfer.Amount);

    /// <summary>
    /// Total amount expected from this source during the Accounting Period.
    /// </summary>
    public decimal ExpectedAmount => NetAmount * ExpectedDates.Count;

    /// <summary>
    /// Total amount expected to reach tracked accounts during the Accounting Period.
    /// </summary>
    public decimal ExpectedTrackedAmount => TrackedAmount * ExpectedDates.Count;

    /// <summary>
    /// Total amount expected to reach untracked accounts during the Accounting Period.
    /// </summary>
    public decimal ExpectedUntrackedAmount => UntrackedAmount * ExpectedDates.Count;

    /// <summary>
    /// Constructs an expected income source.
    /// </summary>
    internal ExpectedIncomeSource(
        AccountingPeriod accountingPeriod,
        string name,
        IEnumerable<IncomeLine> incomeLines,
        IEnumerable<IncomeDeduction> incomeDeductions,
        IEnumerable<ExpectedUntrackedIncomeTransfer> untrackedTransfers,
        IEnumerable<DateOnly> expectedDates)
        : base(new ExpectedIncomeSourceId(Guid.NewGuid()))
    {
        AccountingPeriod = accountingPeriod;
        Name = name;
        _incomeLines.AddRange(incomeLines);
        _incomeDeductions.AddRange(incomeDeductions);
        _untrackedTransfers.AddRange(untrackedTransfers);
        _expectedDates.AddRange(expectedDates.Select(date => new ExpectedIncomeDate(date)));
    }

    /// <summary>
    /// Updates this expected income source.
    /// </summary>
    internal void Update(
        string name,
        IEnumerable<IncomeLine> incomeLines,
        IEnumerable<IncomeDeduction> incomeDeductions,
        IEnumerable<ExpectedUntrackedIncomeTransfer> untrackedTransfers,
        IEnumerable<DateOnly> expectedDates)
    {
        Name = name;
        IncomeLines = incomeLines.ToList();
        IncomeDeductions = incomeDeductions.ToList();
        UntrackedTransfers = untrackedTransfers.ToList();
        ExpectedDates = expectedDates.Select(date => new ExpectedIncomeDate(date)).ToList();
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
/// Identifier for an <see cref="ExpectedIncomeSource"/>.
/// </summary>
public sealed record ExpectedIncomeSourceId : EntityId
{
    internal ExpectedIncomeSourceId(Guid value) : base(value) { }
}
