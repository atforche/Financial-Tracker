using System.Text.Json.Serialization;

namespace Rest.Models.AccountingPeriod;

/// <summary>
/// REST model representing an Accounting Period
/// </summary>
public class AccountingPeriodModel
{
    /// <inheritdoc cref="Domain.Entity.Id"/>
    public Guid Id { get; init; }

    /// <inheritdoc cref="Domain.AccountingPeriods.AccountingPeriod.Year"/>
    public int Year { get; init; }

    /// <inheritdoc cref="Domain.AccountingPeriods.AccountingPeriod.Month"/>
    public int Month { get; init; }

    /// <inheritdoc cref="Domain.AccountingPeriods.AccountingPeriod.IsOpen"/>
    public bool IsOpen { get; init; }

    /// <inheritdoc cref="Domain.AccountingPeriods.AccountingPeriod.Transactions"/>
    public IReadOnlyCollection<TransactionModel> Transactions { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public AccountingPeriodModel(Guid id, int year, int month, bool isOpen, IReadOnlyCollection<TransactionModel> transactions)
    {
        Id = id;
        Year = year;
        Month = month;
        IsOpen = isOpen;
        Transactions = transactions.ToList();
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period entity to build this Accounting Period REST model from</param>
    public AccountingPeriodModel(Domain.AccountingPeriods.AccountingPeriod accountingPeriod)
    {
        Id = accountingPeriod.Id.Value;
        Year = accountingPeriod.Year;
        Month = accountingPeriod.Month;
        IsOpen = accountingPeriod.IsOpen;
        Transactions = accountingPeriod.Transactions.Select(transaction => new TransactionModel(transaction)).ToList();
    }
}