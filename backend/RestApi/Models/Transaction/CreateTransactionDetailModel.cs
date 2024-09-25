namespace RestApi.Models.Transaction;

/// <summary>
/// REST model representing a request to create a Transaction Detail
/// </summary>
public class CreateTransactionDetailModel
{
    /// <see cref="Domain.ValueObjects.TransactionDetail.AccountId"/>
    public required Guid AccountId { get; init; }

    /// <see cref="Domain.ValueObjects.TransactionDetail.StatementDate"/>
    public DateOnly? StatementDate { get; init; }

    /// <see cref="Domain.ValueObjects.TransactionDetail.IsPosted"/>
    public required bool IsPosted { get; init; }
}