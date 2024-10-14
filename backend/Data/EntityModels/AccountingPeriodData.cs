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

    /// <summary>
    /// Id for this Accounting Period
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Year for this Accounting Period
    /// </summary>
    public required int Year { get; set; }

    /// <summary>
    /// Month for this Accounting Period
    /// </summary>
    public required int Month { get; set; }

    /// <summary>
    /// Is open flag for this Accounting Period
    /// </summary>
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