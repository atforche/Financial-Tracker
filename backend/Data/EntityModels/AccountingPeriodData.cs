using Domain.Entities;

namespace Data.EntityModels;

/// <summary>
/// Data model representing an Accounting Period
/// </summary>
public class AccountingPeriodData : IEntityDataModel<AccountingPeriodData>
{
    /// <summary>
    /// Database primary key for this Accounting Period
    /// </summary>
    public long PrimaryKey { get; set; }

    /// <inheritdoc cref="AccountingPeriod.Id"/>
    public required Guid Id { get; set; }

    /// <inheritdoc cref="AccountingPeriod.Year"/>
    public required int Year { get; set; }

    /// <inheritdoc cref="AccountingPeriod.Month"/>
    public required int Month { get; set; }

    /// <inheritdoc cref="AccountingPeriod.IsOpen"/>
    public required bool IsOpen { get; set; }

    /// <inheritdoc/>
    public void Replace(AccountingPeriodData newModel)
    {
        Id = newModel.Id;
        Year = newModel.Year;
        Month = newModel.Month;
        IsOpen = newModel.IsOpen;
    }
}