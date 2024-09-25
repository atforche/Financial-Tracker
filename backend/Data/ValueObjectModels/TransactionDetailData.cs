using Domain.ValueObjects;

namespace Data.ValueObjectModels;

/// <summary>
/// Data model representing a Transaction Detail
/// </summary>
public class TransactionDetailData
{
    /// <summary>
    /// Database primary key for this Transaction Detail
    /// </summary>
    public long PrimaryKey { get; set; }

    /// <see cref="TransactionDetail.AccountId"/>
    public required Guid AccountId { get; set; }

    /// <see cref="TransactionDetail.StatementDate"/>
    public DateOnly? StatementDate { get; set; }

    /// <see cref="TransactionDetail.IsPosted"/>
    public required bool IsPosted { get; set; }
}