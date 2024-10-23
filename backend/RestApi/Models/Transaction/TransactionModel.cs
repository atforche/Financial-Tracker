using RestApi.Models.FundAmount;

namespace RestApi.Models.Transaction;

/// <summary>
/// REST model representing a Transaction
/// </summary>
public class TransactionModel
{
    /// <inheritdoc cref="Domain.Entities.Transaction.Id"/>
    public required Guid Id { get; init; }

    /// <inheritdoc cref="Domain.Entities.Transaction.AccountingDate"/>
    public required DateOnly AccountingDate { get; init; }

    /// <inheritdoc cref="Domain.Entities.Transaction.DebitDetail"/>
    public TransactionDetailModel? DebitDetail { get; init; }

    /// <inheritdoc cref="Domain.Entities.Transaction.CreditDetail"/>
    public TransactionDetailModel? CreditDetail { get; init; }

    /// <inheritdoc cref="Domain.Entities.Transaction.AccountingEntries"/>
    public required ICollection<FundAmountModel> AccountingEntries { get; init; }

    /// <summary>
    /// Converts the Transaction domain entity into a Transaction REST model
    /// </summary>
    /// <param name="transaction">Transaction domain entity to be converted</param>
    /// <returns>The converted Transaction REST model</returns>
    internal static TransactionModel ConvertEntityToModel(Domain.Entities.Transaction transaction) =>
        new TransactionModel
        {
            Id = transaction.Id,
            AccountingDate = transaction.AccountingDate,
            DebitDetail = transaction.DebitDetail != null
                ? TransactionDetailModel.ConvertEntityToModel(transaction.DebitDetail)
                : null,
            CreditDetail = transaction.CreditDetail != null
                ? TransactionDetailModel.ConvertEntityToModel(transaction.CreditDetail)
                : null,
            AccountingEntries = transaction.AccountingEntries
                .Select(FundAmountModel.ConvertValueObjectToModel).ToList()
        };
}