using Domain.Factories;

namespace Domain.Entities;

/// <summary>
/// Entity representing an Accounting Period
/// </summary>
public class AccountingPeriod
{
    /// <summary>
    /// Id for this Accounting Period
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Year for this Accounting Period
    /// </summary>
    public int Year { get; }

    /// <summary>
    /// Month for this Accounting Period
    /// </summary>
    public int Month { get; }

    /// <summary>
    /// Is open flag for this Accounting Period
    /// </summary>
    public bool IsOpen { get; internal set; }

    /// <summary>
    /// Verifies that the current Accounting Period is valid
    /// </summary>
    public void Validate()
    {
        if (Id == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (Year < 2020 || Year > 2050)
        {
            throw new InvalidOperationException();
        }
        if (Month <= 0 || Month > 12)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Factory responsible for constructing instances of an Accounting Period
    /// </summary>
    public class AccountingPeriodFactory : IAccountingPeriodFactory
    {
        /// <inheritdoc/>
        public AccountingPeriod Recreate(IRecreateAccountingPeriodRequest request) =>
            new AccountingPeriod(request.Id, request.Year, request.Month, request.IsOpen);
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="id">Id for this Accounting Period</param>
    /// <param name="year">Year for this Accounting Period</param>
    /// <param name="month">Month for this Accounting Period</param>
    /// <param name="isOpen">Is open flag for this Accounting Period</param>
    internal AccountingPeriod(Guid id, int year, int month, bool isOpen)
    {
        Id = id;
        Year = year;
        Month = month;
        IsOpen = isOpen;
    }
}