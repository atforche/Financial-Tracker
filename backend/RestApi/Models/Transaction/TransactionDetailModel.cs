namespace RestApi.Models.Transaction;

/// <summary>
/// REST model representing a Transaction Detail
/// </summary>
public class TransactionDetailModel
{
    /// <inheritdoc cref="Domain.Entities.TransactionDetail.AccountId"/>
    public required Guid AccountId { get; init; }

    /// <inheritdoc cref="Domain.Entities.TransactionDetail.StatementDate"/>
    public DateOnly? StatementDate { get; init; }

    /// <inheritdoc cref="Domain.Entities.TransactionDetail.IsPosted"/>
    public required bool IsPosted { get; init; }
}