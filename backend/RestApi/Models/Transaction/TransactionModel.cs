using Domain.Entities;

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

    /// <see cref="Domain.Entities.Transaction.StatementDate"/>
    public string? StatementDate { get; init; }

    /// <see cref="Domain.Entities.Transaction.Type"/>
    public required TransactionType Type { get; init; }

    /// <see cref="Domain.Entities.Transaction.IsPosted"/>
    public required bool IsPosted { get; init; }

    /// <see cref="Domain.Entities.Transaction.AccountId"/>
    public required Guid AccountId { get; init; }

    /// <see cref="Domain.Entities.Transaction.AccountingEntries"/>
    public required ICollection<AccountingEntryModel> AccountingEntries { get; init; }
}