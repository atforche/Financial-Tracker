using Domain.Accounts;
using Domain.Locations;

namespace Domain.Transactions.Income;

/// <summary>
/// Value object representing the source of money for an income transaction.
/// </summary>
public class IncomeTransactionSource
{
    private readonly List<IncomeLine> _incomeLines = [];
    private readonly List<IncomeDeduction> _incomeDeductions = [];

    /// <summary>
    /// Account of the income transaction source.
    /// </summary>
    public Account? Account { get; private set; }

    /// <summary>
    /// Posted Date of the income transaction source.
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Location of the income transaction source.
    /// </summary>
    public Location? Location { get; private set; }

    /// <summary>
    /// Income Lines for this income transaction source
    /// </summary>
    public IReadOnlyCollection<IncomeLine> IncomeLines => _incomeLines;

    /// <summary>
    /// Income Deductions for this income transaction source
    /// </summary>
    public IReadOnlyCollection<IncomeDeduction> IncomeDeductions => _incomeDeductions;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public IncomeTransactionSource(
        Account? account,
        DateOnly? postedDate,
        Location? location,
        IEnumerable<IncomeLine> incomeLines,
        IEnumerable<IncomeDeduction> incomeDeductions)
    {
        Account = account;
        PostedDate = postedDate;
        Location = location;
        _incomeLines.AddRange(incomeLines);
        _incomeDeductions.AddRange(incomeDeductions);
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private IncomeTransactionSource()
    {
        Account = null;
    }
}