using System.Diagnostics.CodeAnalysis;

namespace Domain.AccountingPeriods;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="AccountingPeriod"/>
/// </summary>
public interface IAccountingPeriodRepository
{
    /// <summary>
    /// Gets all the Accounting Periods currently in the repository
    /// </summary>
    IReadOnlyCollection<AccountingPeriod> GetAll(GetAllAccountingPeriodsRequest request);

    /// <summary>
    /// Gets the Accounting Period with the specified ID.
    /// </summary>
    AccountingPeriod GetById(AccountingPeriodId id);

    /// <summary>
    /// Attempts to get the Accounting Period with the specified ID.
    /// </summary>
    bool TryGetById(Guid id, [NotNullWhen(true)] out AccountingPeriod? accountingPeriod);

    /// <summary>
    /// Gets the latest Accounting Period
    /// </summary>
    AccountingPeriod? GetLatestAccountingPeriod();

    /// <summary>
    /// Gets the next Accounting Period for the Accounting Period with the specified ID
    /// </summary>
    AccountingPeriod? GetNextAccountingPeriod(AccountingPeriodId id);

    /// <summary>
    /// Gets the previous Accounting Period for the Accounting Period with the specified ID
    /// </summary>
    AccountingPeriod? GetPreviousAccountingPeriod(AccountingPeriodId id);

    /// <summary>
    /// Gets all the Accounting Periods that are currently open
    /// </summary>
    IReadOnlyCollection<AccountingPeriod> GetAllOpenPeriods();

    /// <summary>
    /// Adds the provided Accounting Period to the repository
    /// </summary>
    void Add(AccountingPeriod accountingPeriod);

    /// <summary>
    /// Deletes the provided Accounting Period from the repository
    /// </summary>
    void Delete(AccountingPeriod accountingPeriod);
}

/// <summary>
/// Request to retrieve all the Accounting Periods that match the specified criteria
/// </summary>
public record GetAllAccountingPeriodsRequest
{
    /// <summary>
    /// Sort order to apply to the results
    /// </summary>
    public AccountingPeriodSortOrder? SortBy { get; init; }

    /// <summary>
    /// Years to include in the results
    /// </summary>
    public IReadOnlyCollection<int>? Years { get; init; }

    /// <summary>
    /// Months to include in the results
    /// </summary>
    public IReadOnlyCollection<int>? Months { get; init; }

    /// <summary>
    /// True to include only open Accounting Periods in the results, false to include only closed Accounting Periods, null to include all Accounting Periods
    /// </summary>
    public bool? IsOpen { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}