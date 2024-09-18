using Domain.Entities;

namespace Data.Models;

/// <summary>
/// Data model representing an Accounting Entry
/// </summary>
public class AccountingEntryData : Entity, IDataModel<AccountingEntryData>
{
    /// <summary>
    /// Database primary key for this Accounting Entry
    /// </summary>
    public long PrimaryKey { get; set; }

    /// <see cref="AccountingEntry.Id"/>
    public required Guid Id { get; set; }

    /// <see cref="AccountingEntry.Type"/>
    public required AccountingEntryType Type { get; set; }

    /// <see cref="AccountingEntry.Amount"/>
    public required decimal Amount { get; set; }

    /// <inheritdoc/>
    public void Replace(AccountingEntryData newModel)
    {
        Id = newModel.Id;
        Type = newModel.Type;
        Amount = newModel.Amount;
    }
}