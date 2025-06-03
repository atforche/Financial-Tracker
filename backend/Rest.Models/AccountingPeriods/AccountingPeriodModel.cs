using System.Text.Json.Serialization;
using Domain.AccountingPeriods;

namespace Rest.Models.AccountingPeriods;

/// <summary>
/// REST model representing an <see cref="AccountingPeriod"/>
/// </summary>
public class AccountingPeriodModel
{
    /// <inheritdoc cref="AccountingPeriodId"/>
    public Guid Id { get; init; }

    /// <inheritdoc cref="AccountingPeriod.Year"/>
    public int Year { get; init; }

    /// <inheritdoc cref="AccountingPeriod.Month"/>
    public int Month { get; init; }

    /// <inheritdoc cref="AccountingPeriod.IsOpen"/>
    public bool IsOpen { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public AccountingPeriodModel(Guid id, int year, int month, bool isOpen)
    {
        Id = id;
        Year = year;
        Month = month;
        IsOpen = isOpen;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period entity to build this Accounting Period REST model from</param>
    public AccountingPeriodModel(AccountingPeriod accountingPeriod)
    {
        Id = accountingPeriod.Id.Value;
        Year = accountingPeriod.Year;
        Month = accountingPeriod.Month;
        IsOpen = accountingPeriod.IsOpen;
    }
}