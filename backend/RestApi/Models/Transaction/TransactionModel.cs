namespace RestApi.Models.Transaction;

/// <summary>
/// REST model representing a Transaction
/// </summary>
public class TransactionModel
{
    /// <inheritdoc cref="Domain.Entities.Transaction.Id"/>
    public required Guid Id { get; init; }

    /// <inheritdoc cref="Domain.Entities.Transaction.AccountingDate"/>
    public required string AccountingDate { get; init; }

    /// <inheritdoc cref="Domain.Entities.Transaction.DebitDetail"/>
    public TransactionDetailModel? DebitDetail { get; init; }

    /// <inheritdoc cref="Domain.Entities.Transaction.CreditDetail"/>
    public TransactionDetailModel? CreditDetail { get; init; }

    /// <inheritdoc cref="Domain.Entities.Transaction.AccountingEntries"/>
    public required ICollection<AccountingEntryModel> AccountingEntries { get; init; }
}