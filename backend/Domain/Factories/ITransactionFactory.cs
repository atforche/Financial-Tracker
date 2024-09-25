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

    /// <see cref="Transaction.DebitDetail"/>
    public CreateTransactionDetailRequest? DebitDetail { get; init; }

    /// <see cref="Transaction.CreditDetail"/>
    public CreateTransactionDetailRequest? CreditDetail { get; init; }

    /// <see cref="Transaction.AccountingEntries"/>
    public required IEnumerable<decimal> AccountingEntries { get; init; }
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

    /// <see cref="Transaction.DebitDetail"/>
    IRecreateTransactionDetailRequest? DebitDetail { get; }

    /// <see cref="Transaction.CreditDetail"/>
    IRecreateTransactionDetailRequest? CreditDetail { get; }

    /// <see cref="Transaction.AccountingEntries"/>
    IEnumerable<decimal> AccountingEntries { get; }
}