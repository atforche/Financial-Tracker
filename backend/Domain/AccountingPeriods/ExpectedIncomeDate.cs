namespace Domain.AccountingPeriods;

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