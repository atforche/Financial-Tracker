namespace RestApi.Models.Transaction;

/// <summary>
/// REST model representing a Transaction
/// </summary>
public class TransactionModel
{
    /// <see cref="Domain.Entities.Transaction.Id"/>
    public required Guid Id { get; init; }

    /// <see cref="Domain.Entities.Transaction.AccountingDate"/>
    public required string AccountingDate { get; init; }

    /// <see cref="Domain.Entities.Transaction.DebitDetail"/>
    public TransactionDetailModel? DebitDetail { get; init; }

    /// <see cref="Domain.Entities.Transaction.CreditDetail"/>
    public TransactionDetailModel? CreditDetail { get; init; }

    /// <see cref="Domain.Entities.Transaction.AccountingEntries"/>
    public required ICollection<AccountingEntryModel> AccountingEntries { get; init; }
}