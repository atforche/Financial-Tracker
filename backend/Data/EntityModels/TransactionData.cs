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

    /// <inheritdoc cref="Transaction.Id"/>
    public required Guid Id { get; set; }

    /// <inheritdoc cref="Transaction.AccountingDate"/>
    public required DateOnly AccountingDate { get; set; }

    /// <inheritdoc cref="Transaction.DebitDetail"/>
    public TransactionDetailData? DebitDetail { get; set; }

    /// <inheritdoc cref="Transaction.CreditDetail"/>
    public TransactionDetailData? CreditDetail { get; set; }

    /// <inheritdoc cref="Transaction.AccountingEntries"/>
    public required ICollection<FundAmountData> AccountingEntries { get; init; }

    /// <inheritdoc/>
    public void Replace(TransactionData newModel)
    {
        Id = newModel.Id;
        AccountingDate = newModel.AccountingDate;
        DebitDetail = newModel.DebitDetail;
        CreditDetail = newModel.CreditDetail;
        AccountingEntries.Clear();
        foreach (FundAmountData accountingEntryData in newModel.AccountingEntries)
        {
            AccountingEntries.Add(accountingEntryData);
        }
    }
}