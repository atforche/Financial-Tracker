namespace RestApi.Models.Transaction;

/// <summary>
/// REST model representing a request to create a Transaction
/// </summary>
public class CreateTransactionModel
{
    /// <inheritdoc cref="Domain.Entities.Transaction.AccountingDate"/>
    public required DateOnly AccountingDate { get; init; }

    /// <inheritdoc cref="Domain.Entities.Transaction.DebitDetail"/>
    public CreateTransactionDetailModel? DebitDetail { get; init; }

    /// <inheritdoc cref="Domain.Entities.Transaction.CreditDetail"/>
    public CreateTransactionDetailModel? CreditDetail { get; init; }

    /// <inheritdoc cref="Domain.Entities.Transaction.AccountingEntries"/>
    public required ICollection<CreateAccountingEntryModel> AccountingEntries { get; init; }
}