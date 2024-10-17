using Domain.Entities;

namespace Data.EntityModels;

/// <summary>
/// Data model representing a Transaction Detail
/// </summary>
public class TransactionDetailData
{
    /// <summary>
    /// Database primary key for this Transaction Detail
    /// </summary>
    public long PrimaryKey { get; set; }

    /// <inheritdoc cref="TransactionDetail.Id"/>
    public required Guid Id { get; set; }

    /// <inheritdoc cref="TransactionDetail.AccountId"/>
    public required Guid AccountId { get; set; }

    /// <inheritdoc cref="TransactionDetail.StatementDate"/>
    public DateOnly? StatementDate { get; set; }

    /// <inheritdoc cref="TransactionDetail.IsPosted"/>
    public required bool IsPosted { get; set; }
}