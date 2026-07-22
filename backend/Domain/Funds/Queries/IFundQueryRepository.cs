using Domain.AccountingPeriods.Queries;

namespace Domain.Funds.Queries;

/// <summary>
/// Interface for retrieving persisted Fund facts used by read operations.
/// </summary>
public interface IFundQueryRepository
{
    /// <summary>
    /// Retrieves the Funds matching the provided query.
    /// </summary>
    Task<QueryPage<Fund>> GetAsync(FundQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Funds and their current balances matching the provided query.
    /// </summary>
    Task<QueryPage<FundBalance>> GetBalancesAsync(
        FundBalanceQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Funds matching the provided range filter.
    /// </summary>
    Task<IReadOnlyCollection<Fund>> GetRangeFundsAsync(FundFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all available Fund names.
    /// </summary>
    Task<IReadOnlyCollection<string>> GetAllNamesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Fund balance facts over the provided chronological range.
    /// </summary>
    Task<IReadOnlyCollection<FundPeriodBalanceFacts>> GetPeriodBalanceFactsAsync(
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Fund boundary balances for a date range.
    /// </summary>
    Task<IReadOnlyCollection<FundRangeBalance>> GetDateRangeBalancesAsync(
        FundFilter filter,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Fund balance history facts through the provided date.
    /// </summary>
    Task<IReadOnlyCollection<FundDateBalanceFact>> GetDateBalanceFactsAsync(
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves income destination facts within the provided date range.
    /// </summary>
    Task<IReadOnlyCollection<FinancialRangeIncomeFact>> GetDateRangeIncomeFactsAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves spending facts within the provided date range.
    /// </summary>
    Task<IReadOnlyCollection<FinancialRangeSpendingFact>> GetDateRangeSpendingFactsAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);
}