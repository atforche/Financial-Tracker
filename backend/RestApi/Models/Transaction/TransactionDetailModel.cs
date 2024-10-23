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

    /// <summary>
    /// Converts the Transaction Detail domain entity into a Transaction Detail REST model
    /// </summary>
    /// <param name="transactionDetail">Transaction Detail domain entity to be converted</param>
    /// <returns>The converted Transaction Detail REST model</returns>
    internal static TransactionDetailModel ConvertEntityToModel(Domain.Entities.TransactionDetail transactionDetail) =>
        new TransactionDetailModel
        {
            AccountId = transactionDetail.AccountId,
            StatementDate = transactionDetail.StatementDate,
            IsPosted = transactionDetail.IsPosted,
        };
}