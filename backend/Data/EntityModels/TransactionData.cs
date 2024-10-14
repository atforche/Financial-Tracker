using Data.ValueObjectModels;
using Domain.Entities;

namespace Data.EntityModels;

/// <summary>
/// Data model representing a Transaction
/// </summary>
public class TransactionData : IEntityDataModel<TransactionData>
{
    /// <summary>
    /// Database primary key for this Transaction
    /// </summary>
    public long PrimaryKey { get; set; }

    /// <see cref="Transaction.Id"/>
    public required Guid Id { get; set; }

    /// <see cref="Transaction.AccountingDate"/>
    public required DateOnly AccountingDate { get; set; }

    /// <see cref="Transaction.DebitDetail"/>
    public TransactionDetailData? DebitDetail { get; set; }

    /// <see cref="Transaction.CreditDetail"/>
    public TransactionDetailData? CreditDetail { get; set; }

    /// <see cref="Transaction.AccountingEntries"/>
    public required ICollection<AccountingEntryData> AccountingEntries { get; init; }

    /// <inheritdoc/>
    public void Replace(TransactionData newModel)
    {
        Id = newModel.Id;
        AccountingDate = newModel.AccountingDate;
        DebitDetail = newModel.DebitDetail;
        CreditDetail = newModel.CreditDetail;

        AccountingEntries.Clear();
        foreach (AccountingEntryData accountingEntryData in newModel.AccountingEntries)
        {
            AccountingEntries.Add(accountingEntryData);
        }
    }
}