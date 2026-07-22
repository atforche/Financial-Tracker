using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.FundPlans.Queries;

/// <summary>
/// Defines read-only persistence operations for Fund Plans.
/// </summary>
public interface IFundPlanQueryRepository
{
    /// <summary>
    /// Retrieves the Fund Plan with the specified ID, or null when it does not exist.
    /// </summary>
    Task<FundPlan?> GetByIdAsync(FundPlanId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Fund Plans matching the provided query.
    /// </summary>
    Task<QueryPage<FundPlan>> GetAsync(FundPlanQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the Fund Plan for the specified Fund and Accounting Period, or null when it does not exist.
    /// </summary>
    Task<FundPlan?> GetByFundAndAccountingPeriodAsync(
        FundId fundId,
        AccountingPeriodId? accountingPeriodId,
        CancellationToken cancellationToken = default);
}