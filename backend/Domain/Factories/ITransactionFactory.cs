using Domain.Entities;

namespace Domain.Factories;

/// <summary>
/// Interface representing a factory responsible for constructing instances of a Transaction
/// </summary>
public interface ITransactionFactory
{
    /// <summary>
    /// Creates a new Transaction with the provided properties
    /// </summary>
    /// <param name="request">Request to create a Transaction</param>
    /// <returns>The newly created Transaction</returns>
    Transaction Create(CreateTransactionRequest request);

    /// <summary>
    /// Recreates an existing Transaction with the provided properties
    /// </summary>
    /// <param name="request">Request to recreate a Transaction</param>
    /// <returns>The recreated Transaction</returns>
    Transaction Recreate(IRecreateTransactionRequest request);
}

/// <summary>
/// Record representing a request to create a Transaction
/// </summary>
public record CreateTransactionRequest
{
    /// <see cref="Transaction.AccountingDate"/>
    public required DateOnly AccountingDate { get; init; }

    /// <see cref="Transaction.StatementDate"/>
    public DateOnly? StatementDate { get; init; }

    /// <see cref="Transaction.Type"/>
    public required TransactionType Type { get; init; }

    /// <see cref="Transaction.IsPosted"/>
    public required bool IsPosted { get; init; }

    /// <see cref="Transaction.AccountId"/>
    public required Guid AccountId { get; init; }

    /// <see cref="Transaction.AccountingEntries"/>
    public required IEnumerable<CreateAccountingEntryRequest> AccountingEntries { get; init; }
}

/// <summary>
/// Interface representing a request to recreate an existing Transaction
/// </summary>
public interface IRecreateTransactionRequest
{
    /// <see cref="Transaction.Id"/>
    Guid Id { get; }

    /// <see cref="Transaction.AccountingDate"/>
    DateOnly AccountingDate { get; }

    /// <see cref="Transaction.StatementDate"/>
    DateOnly? StatementDate { get; }

    /// <see cref="Transaction.Type"/>
    TransactionType Type { get; }

    /// <see cref="Transaction.IsPosted"/>
    bool IsPosted { get; }

    /// <see cref="Transaction.AccountId"/>
    Guid AccountId { get; }

    /// <see cref="Transaction.AccountingEntries"/>
    IEnumerable<IRecreateAccountingEntryRequest> AccountingEntries { get; }
}