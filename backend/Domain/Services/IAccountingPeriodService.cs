using Domain.Entities;

namespace Domain.Services;

/// <summary>
/// Interface representing a service used to create or modify Accounting Periods
/// </summary>
public interface IAccountingPeriodService
{
    /// <summary>
    /// Creates a new Accounting Period with the provided properties
    /// </summary>
    /// <param name="request">Request to create an Accounting Period</param>
    /// <param name="newAccountingPeriod">The newly created Accounting Period</param>
    /// <param name="newAccountStartingBalances">The newly created Account Starting Balances for this Accounting Period</param>
    void CreateNewAccountingPeriod(CreateAccountingPeriodRequest request,
        out AccountingPeriod newAccountingPeriod,
        out ICollection<AccountStartingBalance> newAccountStartingBalances);

    /// <summary>
    /// Closes out the provided Accounting Period
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to be closed</param>
    /// <param name="newAccountStartingBalances">The newly created Account Starting Balances for the future Accounting Period</param>
    void ClosePeriod(
        AccountingPeriod accountingPeriod,
        out ICollection<AccountStartingBalance> newAccountStartingBalances);
}

/// <summary>
/// Record representing a request to create an Accounting Period
/// </summary>
public record CreateAccountingPeriodRequest
{
    /// <inheritdoc cref="AccountingPeriod.Year"/>
    public required int Year { get; init; }

    /// <inheritdoc cref="AccountingPeriod.Month"/>
    public required int Month { get; init; }
}