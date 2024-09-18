using Domain.Entities;

namespace RestApi.Models.Transaction;

/// <summary>
/// REST model representing a request to create a Transaction
/// </summary>
public class CreateTransactionModel
{
    /// <see cref="Domain.Entities.Transaction.AccountingDate"/>
    public required string AccountingDate { get; init; }

    /// <see cref="Domain.Entities.Transaction.StatementDate"/>
    public string? StatementDate { get; init; }

    /// <see cref="Domain.Entities.Transaction.Type"/>
    public TransactionType Type { get; init; }

    /// <see cref="Domain.Entities.Transaction.IsPosted"/>
    public bool IsPosted { get; init; }

    /// <see cref="Domain.Entities.Transaction.AccountId"/>
    public Guid AccountId { get; init; }

    /// <see cref="Domain.Entities.Transaction.AccountingEntries"/>
    public required ICollection<CreateAccountingEntryModel> AccountingEntries { get; init; }
}