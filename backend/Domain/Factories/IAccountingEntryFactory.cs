using Domain.Entities;

namespace Domain.Factories;

/// <summary>
/// Interface representing a factory responsible for constructing instances of an Accounting Entry
/// </summary>
public interface IAccountingEntryFactory
{
    /// <summary>
    /// Creates a new AccountingEntry with the provided properties
    /// </summary>
    /// <param name="request">Request to create an AccountingEntry</param>
    /// <returns>The newly created AccountingEntry</returns>
    AccountingEntry Create(CreateAccountingEntryRequest request);

    /// <summary>
    /// Recreates an existing AccountingEntry with the provided properties
    /// </summary>
    /// <param name="request">Request to recreate an AccountingEntry</param>
    /// <returns>The recreated AccountingEntry</returns>
    AccountingEntry Recreate(IRecreateAccountingEntryRequest request);
}

/// <summary>
/// Record representing a request to create an AccountingEntry
/// </summary>
public record CreateAccountingEntryRequest
{
    /// <summary>
    /// Type for this AccountingEntry
    /// </summary>
    public required AccountingEntryType Type { get; init; }

    /// <summary>
    /// Amount for this AccountingEntry
    /// </summary>
    public required decimal Amount { get; init; }
};

/// <summary>
/// Interface representing a request to recreate an existing AccountingEntry
/// </summary>
public interface IRecreateAccountingEntryRequest
{
    /// <summary>
    /// Id for the AccountingEntry
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Type for this AccountingEntry
    /// </summary>
    AccountingEntryType Type { get; }

    /// <summary>
    /// Amount for this AccountingEntry
    /// </summary>
    decimal Amount { get; }
}