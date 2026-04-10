using System.Globalization;

namespace Domain.AccountingPeriods;

/// <summary>
/// Entity class representing an Accounting Period
/// </summary>
/// <remarks>
/// An Accounting Period represents a month-long period used to organize transactions and track balances and budgets.
/// </remarks>
public class AccountingPeriod : Entity<AccountingPeriodId>
{
    /// <summary>
    /// Year for this Accounting Period
    /// </summary>
    public int Year { get; private set; }

    /// <summary>
    /// Month for this Accounting Period
    /// </summary>
    public int Month { get; private set; }

    /// <summary>
    /// Name for this Accounting Period, in the format of "MMMM yyyy" (e.g. "February 2026")
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the Period Start Date for this Accounting Period
    /// </summary>
    public DateOnly PeriodStartDate => new(Year, Month, 1);

    /// <summary>
    /// Is Open flag for this Accounting Period
    /// </summary>
    /// <remarks>
    /// Once an Accounting Period has been closed, no changes can be made to anything thats fall within 
    /// that Accounting Period. Multiple Accounting Periods can be open at the same time, assuming all 
    /// the open periods represent a contiguous period of time. Only the earliest open period can be closed.
    /// </remarks>
    public bool IsOpen { get; internal set; }

    /// <summary>
    /// Determines if the provided date falls within the Accounting Period
    /// </summary>
    public bool IsDateInPeriod(DateOnly date) => date >= GetMinimumDateInPeriod() && date <= GetMaximumDateInPeriod();

    /// <summary>
    /// Gets the minimum date that falls within this Accounting Period
    /// </summary>
    public DateOnly GetMinimumDateInPeriod() => new DateOnly(Year, Month, 1).AddMonths(-1);

    /// <summary>
    /// Gets the maximum date that falls within this Accounting Period
    /// </summary>
    public DateOnly GetMaximumDateInPeriod() => new DateOnly(Year, Month, 1).AddMonths(1).AddDays(-1);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="year">Year for this Accounting Period</param>
    /// <param name="month">Month for this Accounting Period</param>
    internal AccountingPeriod(int year, int month)
        : base(new AccountingPeriodId(Guid.NewGuid()))
    {
        Year = year;
        Month = month;
        Name = PeriodStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        IsOpen = true;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private AccountingPeriod() : base()
    {
        Name = null!;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="AccountingPeriod"/>
/// </summary>
public record AccountingPeriodId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// </summary>
    internal AccountingPeriodId(Guid value)
        : base(value)
    {
    }
}