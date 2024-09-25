using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Factories;

/// <summary>
/// Interface representing a factory responsible for constructing instances of a Transaction Detail
/// </summary>
public interface ITransactionDetailFactory
{
    /// <summary>
    /// Creates a new Transaction Detail with the provided properties
    /// </summary>
    /// <param name="request">Request to create a Transaction Detail</param>
    /// <returns>The newly created Transaction Detail</returns>
    TransactionDetail Create(CreateTransactionDetailRequest request);

    /// <summary>
    /// Recreates an existing Transaction Detail with the provided properties
    /// </summary>
    /// <param name="request">Request to recreate a Transaction Detail</param>
    /// <returns>The recreated Transaction Detail</returns>
    TransactionDetail Recreate(IRecreateTransactionDetailRequest request);
}

/// <summary>
/// Record representing a request to create a Transaction Detail
/// </summary>
public record CreateTransactionDetailRequest
{
    /// <see cref="TransactionDetail.AccountId"/>
    public required Account Account { get; init; }

    /// <see cref="TransactionDetail.StatementDate"/>
    public DateOnly? StatementDate { get; init; }

    /// <see cref="TransactionDetail.IsPosted"/>
    public required bool IsPosted { get; init; }
}

/// <summary>
/// Interface representing a request to recreate an existing Transaction Detail
/// </summary>
public interface IRecreateTransactionDetailRequest
{
    /// <see cref="TransactionDetail.AccountId"/>
    Guid AccountId { get; }

    /// <see cref="TransactionDetail.StatementDate"/>
    DateOnly? StatementDate { get; }

    /// <see cref="TransactionDetail.IsPosted"/>
    bool IsPosted { get; }
}