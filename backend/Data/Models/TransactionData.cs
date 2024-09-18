using Domain.Entities;

namespace Data.Models;

/// <summary>
/// Data model representing a Transaction
/// </summary>
public class TransactionData : Entity, IDataModel<TransactionData>
{
    /// <summary>
    /// Database primary key for this Transaction
    /// </summary>
    public long PrimaryKey { get; set; }

    /// <see cref="Transaction.Id"/>
    public required Guid Id { get; set; }

    /// <see cref="Transaction.AccountingDate"/>
    public required DateOnly AccountingDate { get; set; }

    /// <see cref="Transaction.StatementDate"/>
    public DateOnly? StatementDate { get; set; }

    /// <see cref="Transaction.Type"/>
    public required TransactionType Type { get; set; }

    /// <see cref="Transaction.IsPosted"/>
    public required bool IsPosted { get; set; }

    /// <see cref="Transaction.AccountId"/>
    public required Guid AccountId { get; set; }

    /// <see cref="Transaction.AccountingEntries"/>
    public required ICollection<AccountingEntryData> AccountingEntries { get; init; }

    /// <inheritdoc/>
    public void Replace(TransactionData newModel)
    {
        Id = newModel.Id;
        AccountingDate = newModel.AccountingDate;
        StatementDate = newModel.StatementDate;
        Type = newModel.Type;
        IsPosted = newModel.IsPosted;
        AccountId = newModel.AccountId;
        foreach (AccountingEntryData newEntry in newModel.AccountingEntries)
        {
            AccountingEntryData? existingEntry = AccountingEntries.SingleOrDefault(entry => entry.Id == newEntry.Id);
            if (existingEntry != null)
            {
                existingEntry.Replace(newEntry);
            }
            else
            {
                AccountingEntries.Add(newEntry);
            }
        }
    }
}