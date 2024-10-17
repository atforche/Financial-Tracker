using Domain.ValueObjects;

namespace Data.ValueObjectModels;

/// <summary>
/// Data model representing an Accounting Entry
/// </summary>
public class AccountingEntryData
{
    /// <summary>
    /// Database primary key for this Accounting Entry
    /// </summary>
    public long PrimaryKey { get; set; }

    /// <inheritdoc cref="AccountingEntry.Amount"/>
    public required decimal Amount { get; set; }
}